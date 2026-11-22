const ctx = Shell.init({ roles: ['manager'], active: 'tickets', crumbs: project => [project.name, ['Входящие', 'tickets.html'], 'Изменение'] });
const found = ctx ? Store.ticket(ctx.params.get('id')) : null;
if (ctx && (!found || found.projectId !== ctx.project.id || ['На проверке', 'Завершён'].includes(found.status))) location.replace('tickets.html');
else if (ctx) {
	const { $ } = UI;
	const id = found.id;
	const form = $('#ticket-form');
	const alertBox = $('#form-alert');
	$('#te-sub').textContent = `${found.number} · ${found.title}`;
	$('#cancel-link').href = `ticket.html?id=${id}`;

	form.projectId.innerHTML = Store.managedProjects().map(p => `<option value="${p.id}">${UI.esc(p.name)}</option>`).join('');
	form.projectId.value = found.projectId;
	form.title.value = found.title;
	form.description.value = found.description;
	form.steps.value = found.steps;
	form.expected.value = found.expected;
	form.actual.value = found.actual;
	form.priority.value = found.priority;
	form.startDate.value = found.startAt.slice(0, 10);
	form.startTime.value = found.startAt.slice(11, 16);
	form.dueDate.value = found.dueAt.slice(0, 10);
	form.dueTime.value = found.dueAt.slice(11, 16);

	const files = UI.attachments({ button: $('#attach-btn'), input: $('#attach-input'), list: $('#attach-list'), errorBox: $('#attach-errors'), initial: found.attachments });

	const submit = () => {
		const result = Store.updateTicket(id, {
			title: form.title.value, description: form.description.value, steps: form.steps.value,
			expected: form.expected.value, actual: form.actual.value, comment: form.comment.value,
			projectId: form.projectId.value, priority: form.priority.value,
			startDate: form.startDate.value, startTime: form.startTime.value,
			dueDate: form.dueDate.value, dueTime: form.dueTime.value
		}, files.get());
		if (!result.ok) {
			UI.applyErrors(form, result.errors);
			const count = Object.keys(result.errors).length;
			$('#alert-title').textContent = 'Изменения не сохранены';
			$('#alert-text').textContent = `Проверьте выделенные поля (${count}). Текущие данные тикета не изменились.`;
			alertBox.classList.remove('hidden');
			window.scrollTo({ top: 0, behavior: 'smooth' });
			return;
		}
		UI.flash('Изменения сохранены');
		location.href = result.ticket.projectId === found.projectId ? `ticket.html?id=${id}` : 'tickets.html';
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
