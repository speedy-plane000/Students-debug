const ctx = Shell.init({ roles: ['manager'], active: 'tickets', crumbs: project => [project.name, 'Тикеты'] });
if (ctx) {
	const { $, esc } = UI;
	const project = ctx.project;
	const TABS = [
		{ key: 'created', label: 'Созданные', match: t => t.status === 'На рассмотрении' },
		{ key: 'dev', label: 'У разработчиков', match: t => ['Назначен', 'В работе', 'На доработке', 'Приостановлен', 'В общем списке'].includes(t.status) },
		{ key: 'review', label: 'На проверке', match: t => t.status === 'На проверке' },
		{ key: 'all', label: 'Все тикеты', match: () => true }
	];
	const STATUSES = ['На рассмотрении', 'В общем списке', 'Назначен', 'В работе', 'На проверке', 'На доработке', 'Приостановлен', 'Завершён'];
	let tab = TABS.some(t => t.key === ctx.params.get('tab')) ? ctx.params.get('tab') : 'created';

	$('#tickets-sub').textContent = `${project.name} · Проверьте описание и направьте тикет разработчику.`;
	$('#f-status').innerHTML = `<option value="">Все статусы</option>${STATUSES.map(s => `<option>${s}</option>`).join('')}`;

	const banner = () => {
		const latest = Store.tickets(project.id).find(t => t.status === 'На рассмотрении');
		$('#incoming-banner').classList.toggle('hidden', !latest);
		if (!latest) return;
		$('#banner-title').textContent = `${latest.number} создан и отправлен руководителю`;
		$('#banner-text').textContent = `${Store.user(latest.authorId).name} добавил(а) тикет «${latest.title}» · ${UI.fmtShort(latest.createdAt)}.`;
	};

	const renderTabs = () => {
		const all = Store.tickets(project.id);
		$('#ticket-tabs').innerHTML = TABS.map(t => `<a href="#" data-tab="${t.key}"${t.key === tab ? ' class="active"' : ''}>${t.label} · ${all.filter(t.match).length}</a>`).join('');
	};

	const renderRows = () => {
		const current = TABS.find(t => t.key === tab);
		const base = Store.tickets(project.id).filter(current.match);
		const query = $('#q').value.trim().toLowerCase();
		const status = $('#f-status').value;
		const priority = $('#f-priority').value;
		const rows = base.filter(t => (!query || t.number.toLowerCase().includes(query) || t.title.toLowerCase().includes(query)) && (!status || t.status === status) && (!priority || t.priority === priority));
		$('#rows').innerHTML = rows.length
			? rows.map(t => `<a class="t-row" href="ticket.html?id=${t.id}"><div><span class="id">${esc(t.number)}</span><span class="name">${esc(t.title)}</span></div><div>${UI.statusChip(t.status)}</div><div class="${UI.priorityClass(t.priority)}">${esc(t.priority)}</div><div>${t.assigneeId ? esc(Store.user(t.assigneeId).name) : 'Не назначен'}</div><div class="muted">${UI.fmtDue(t.dueAt)}</div></a>`).join('')
			: `<div class="empty" style="border:0">${base.length ? '<b>Ничего не найдено</b>Измените условия поиска или фильтры.' : '<b>Тикетов пока нет</b>Здесь появятся тикеты, которые создадут тестировщики.'}</div>`;
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

	banner();
	renderTabs();
	renderRows();
}
