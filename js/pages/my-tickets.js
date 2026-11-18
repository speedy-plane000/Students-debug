const ctx = Shell.init({ roles: ['tester'], active: 'tickets', crumbs: project => [project.name, 'Тикеты'] });
if (ctx) {
	const { $, esc } = UI;
	const project = ctx.project;
	const me = ctx.user.id;
	const STATUSES = ['На рассмотрении', 'В общем списке', 'Назначен', 'В работе', 'На проверке', 'На доработке', 'Приостановлен', 'Завершён'];
	let tab = ctx.params.get('tab') === 'review' ? 'review' : 'mine';

	$('#tickets-sub').textContent = `${project.name} · Созданные вами ошибки и результаты проверки`;
	$('#f-status').innerHTML = `<option value="">Все статусы</option>${STATUSES.map(s => `<option>${s}</option>`).join('')}`;

	const mine = () => Store.tickets(project.id).filter(t => t.authorId === me);
	const sets = { mine: () => mine(), review: () => mine().filter(t => t.status === 'На проверке') };

	const renderTabs = () => {
		$('#ticket-tabs').innerHTML = `<a href="#" data-tab="mine"${tab === 'mine' ? ' class="active"' : ''}>Мои тикеты · ${sets.mine().length}</a><a href="#" data-tab="review"${tab === 'review' ? ' class="active"' : ''}>На проверке · ${sets.review().length}</a>`;
	};

	const renderRows = () => {
		const base = sets[tab]();
		const query = $('#q').value.trim().toLowerCase();
		const status = $('#f-status').value;
		const priority = $('#f-priority').value;
		const rows = base.filter(t => (!query || t.number.toLowerCase().includes(query) || t.title.toLowerCase().includes(query)) && (!status || t.status === status) && (!priority || t.priority === priority));
		$('#rows').innerHTML = rows.length
			? rows.map(t => `<div class="t-row row-static"><div><span class="id">${esc(t.number)}</span><span class="name">${esc(t.title)}</span></div><div>${UI.statusChip(t.status)}</div><div class="${UI.priorityClass(t.priority)}">${esc(t.priority)}</div><div>${t.assigneeId ? esc(Store.user(t.assigneeId).name) : 'Не назначен'}</div><div class="muted">${UI.fmtDue(t.dueAt)}</div></div>`).join('')
			: `<div class="empty" style="border:0">${base.length ? '<b>Ничего не найдено</b>Измените условия поиска или фильтры.' : '<b>Тикетов пока нет</b>Нажмите «Создать тикет», чтобы сообщить об ошибке.'}</div>`;
		$('#shown').textContent = `Показано ${rows.length} из ${base.length} ${UI.plural(base.length, ['тикета', 'тикетов', 'тикетов'])}`;
	};

	$('#ticket-tabs').addEventListener('click', e => {
		const link = e.target.closest('[data-tab]');
		if (!link) return;
		e.preventDefault();
		tab = link.dataset.tab;
		renderTabs();
		renderRows();
	});
	['#q', '#f-status', '#f-priority'].forEach(id => $(id).addEventListener('input', renderRows));

	renderTabs();
	renderRows();
}
