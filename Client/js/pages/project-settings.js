const ctx = Shell.init({ roles: ['manager'], active: 'settings', crumbs: project => [['Проекты', 'projects.html'], project.name, 'Настройки'] });
if (ctx) {
	const { $ } = UI;
	const project = ctx.project;

	const form = $('#project-form');
	const chip = $('#repo-chip');
	const repoField = $('[data-field="repo"]');

	const fill = () => {
		$('#project-title').textContent = project.name;
		form.name.value = project.name;
		form.code.value = project.code;
		form.description.value = project.description;
		form.repo.value = project.repo;
		$('#owner-name').textContent = Store.user(project.ownerId).name;
	};

	const showRepo = state => {
		chip.className = `chip ${state === 'ok' ? 'chip-ok' : 'chip-danger'}${state === 'idle' ? ' hidden' : ''}`;
		chip.textContent = state === 'ok' ? 'Репозиторий доступен' : 'Ошибка проверки';
	};
	const checkRepo = () => {
		const repo = form.repo.value.trim();
		if (!repo) {
			UI.setFieldError(repoField, '');
			showRepo('idle');
			UI.toast('Ссылка на репозиторий не указана');
			return;
		}
		const ok = Store.checkRepository(repo).ok;
		UI.setFieldError(repoField, ok ? '' : 'Репозиторий не найден или недоступен. Проверьте адрес и права доступа.');
		showRepo(ok ? 'ok' : 'error');
	};

	form.code.addEventListener('input', () => {
		form.code.value = form.code.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
	});
	form.repo.addEventListener('input', () => {
		UI.setFieldError(repoField, '');
		showRepo('idle');
	});
	$('#repo-check').addEventListener('click', checkRepo);

	form.addEventListener('submit', e => {
		e.preventDefault();
		const result = Store.updateProject(project.id, {
			name: form.name.value, code: form.code.value, description: form.description.value, repo: form.repo.value
		});
		if (!result.ok) {
			UI.applyErrors(form, result.errors);
			showRepo(result.errors.repo ? 'error' : 'idle');
			return;
		}
		UI.flash('Настройки проекта сохранены');
		location.href = 'projects.html';
	});

	const openCount = Store.tickets(project.id).filter(t => t.status !== 'Завершён').length;
	$('#dp-name').textContent = project.name;
	$('#dp-lead').textContent = `Будут удалены ${openCount} ${UI.plural(openCount, ['открытый тикет', 'открытых тикета', 'открытых тикетов'])}, комментарии и вложения. Участники потеряют доступ к проекту. Это действие нельзя отменить.`;
	const confirmField = $('#confirm-field');
	const confirmInput = $('#confirm-name');
	const deleteProject = () => {
		const result = Store.deleteProject(project.id, confirmInput.value);
		if (!result.ok) {
			UI.setFieldError(confirmField, 'Название не совпадает с названием проекта');
			return;
		}
		UI.flash(`Проект «${project.name}» удалён`);
		location.href = 'projects.html';
	};
	$('#delete-confirm').addEventListener('click', deleteProject);
	confirmInput.addEventListener('input', () => UI.setFieldError(confirmField, ''));
	confirmInput.addEventListener('keydown', e => {
		if (e.key === 'Enter') deleteProject();
	});
	$('[data-open="modal-delete-project"]').addEventListener('click', () => {
		confirmInput.value = '';
		UI.setFieldError(confirmField, '');
	});

	fill();
}
