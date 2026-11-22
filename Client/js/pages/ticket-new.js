const ctx = Shell.init({ roles: ['tester'], active: 'tickets', crumbs: project => [project.name, ['Мои тикеты', 'my-tickets.html'], 'Новый тикет'] });
if (ctx) {
	const { $ } = UI;
	const form = $('#ticket-form');
	const alertBox = $('#form-alert');
	const pad = n => String(n).padStart(2, '0');

	const projects = Store.testerProjects();
	form.projectId.innerHTML = projects.map(p => `<option value="${p.id}">${UI.esc(p.name)}</option>`).join('');
	form.projectId.value = ctx.project.id;

	const now = new Date();
	form.startDate.value = `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`;
	form.startTime.value = `${pad(now.getHours())}:${pad(now.getMinutes())}`;

	const files = UI.attachments({ button: $('#attach-btn'), input: $('#attach-input'), list: $('#attach-list'), errorBox: $('#attach-errors') });

	const readValues = () => ({
		title: form.title.value, description: form.description.value, steps: form.steps.value,
		expected: form.expected.value, actual: form.actual.value, comment: form.comment.value,
		projectId: form.projectId.value, priority: form.priority.value,
		startDate: form.startDate.value, startTime: form.startTime.value,
		dueDate: form.dueDate.value, dueTime: form.dueTime.value
	});

	const submit = () => {
		const result = Store.createTicket(readValues(), files.get());
		if (!result.ok) {
			UI.applyErrors(form, result.errors);
			const count = Object.keys(result.errors).length;
			$('#alert-title').textContent = `Тикет не создан — исправьте ${count} ${UI.plural(count, ['ошибку', 'ошибки', 'ошибок'])}`;
			$('#alert-text').textContent = 'Проверьте выделенные поля. Введённые данные сохранены в форме.';
			alertBox.classList.remove('hidden');
			$('#nt-sub').textContent = 'Проверьте сроки и вложения перед отправкой.';
			window.scrollTo({ top: 0, behavior: 'smooth' });
			return;
		}
		UI.flash(`Тикет ${result.ticket.number} создан и отправлен руководителю`);
		location.href = 'my-tickets.html';
	};

	form.addEventListener('submit', e => {
		e.preventDefault();
		submit();
	});
	$('#submit-top').addEventListener('click', submit);
	form.addEventListener('input', e => {
		const field = e.target.closest('.field');
		if (field && field.classList.contains('invalid')) UI.setFieldError(field, '');
	});
}
