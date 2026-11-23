const ctx = Shell.init({ project: false, active: 'projects', crumbs: [['Проекты', 'projects.html'], 'Новый проект'] });
const { $ } = UI;

const form = $('#project-form');
const chip = $('#repo-chip');
const sub = $('#np-sub');
const subDefault = sub.textContent;
const fail = $('#np-fail');
$('#manager-name').textContent = ctx.user.name;

const values = () => ({
	name: form.name.value,
	code: form.code.value,
	description: form.description.value,
	repo: form.repo.value
});

const showRepo = state => {
	chip.className = `chip ${state === 'ok' ? 'chip-ok' : 'chip-danger'}${state === 'idle' ? ' hidden' : ''}`;
	chip.textContent = state === 'ok' ? 'Репозиторий доступен' : 'Ошибка проверки';
	$('#repo-check').textContent = state === 'error' ? 'Проверить ещё раз' : 'Проверить репозиторий';
	fail.classList.toggle('hidden', state !== 'error');
	sub.textContent = state === 'error' ? 'Не удалось подтвердить Git-репозиторий. Проверьте ссылку ниже.' : subDefault;
};

const checkRepo = () => {
	const repo = form.repo.value.trim();
	if (!repo) {
		UI.setFieldError($('[data-field="repo"]'), '');
		showRepo('idle');
		UI.toast('Ссылка на репозиторий не указана');
		return;
	}
	const ok = Store.checkRepository(repo).ok;
	UI.setFieldError($('[data-field="repo"]'), ok ? '' : 'Репозиторий не найден или недоступен. Проверьте адрес и права доступа.');
	showRepo(ok ? 'ok' : 'error');
};

form.code.addEventListener('input', () => {
	form.code.value = form.code.value.toUpperCase().replace(/[^A-Z0-9]/g, '');
});
form.repo.addEventListener('input', () => {
	UI.setFieldError($('[data-field="repo"]'), '');
	showRepo('idle');
});
$('#repo-check').addEventListener('click', checkRepo);

form.addEventListener('submit', e => {
	e.preventDefault();
	const result = Store.createProject(values());
	if (!result.ok) {
		UI.applyErrors(form, result.errors);
		showRepo(result.errors.repo ? 'error' : 'idle');
		if (result.errors.repo) UI.setFieldError($('[data-field="repo"]'), result.errors.repo);
		return;
	}
	UI.flash(`Проект «${result.project.name}» создан`);
	location.href = 'projects.html';
});
