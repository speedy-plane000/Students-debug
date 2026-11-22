const Dispatch = (() => {
	const now = () => Store.toLocalIso(new Date());
	const newId = () => `h${Date.now().toString(36)}${Math.random().toString(36).slice(2, 6)}`;

	const context = ticketId => {
		const ticket = Store.ticket(ticketId);
		if (!ticket) return { error: 'Тикет не найден' };
		const member = Store.membership(ticket.projectId);
		if (!member || member.role !== 'manager' || member.status !== 'active') return { error: 'Распределять тикеты может только руководитель проекта' };
		if (ticket.status !== 'На рассмотрении') return { error: 'Тикет уже обработан: его статус изменился' };
		return { ticket, project: Store.project(ticket.projectId) };
	};

	const log = (ticketId, text) => Store.db().history.push({ id: newId(), ticketId, text, at: now() });
	const notify = (userId, projectId, text) => Store.db().notifications.unshift({ id: newId(), userId, projectId, text, link: null, at: now(), read: false });
	const activeDevelopers = projectId => Store.db().members.filter(m => m.projectId === projectId && m.role === 'developer' && m.status === 'active');

	const assign = (ticketId, userId) => {
		const ctx = context(ticketId);
		if (ctx.error) return { ok: false, error: ctx.error };
		const target = activeDevelopers(ctx.project.id).find(m => m.userId === userId);
		if (!target) return { ok: false, error: 'Выбранный участник не является разработчиком проекта' };
		ctx.ticket.assigneeId = userId;
		ctx.ticket.status = 'Назначен';
		log(ticketId, `Назначен исполнитель: ${Store.user(userId).name}`);
		notify(userId, ctx.project.id, `Вам назначен тикет ${ctx.ticket.number}`);
		Store.persist();
		return { ok: true };
	};

	const sendToCommonList = ticketId => {
		const ctx = context(ticketId);
		if (ctx.error) return { ok: false, error: ctx.error };
		ctx.ticket.assigneeId = null;
		ctx.ticket.status = 'В общем списке';
		log(ticketId, 'Тикет отправлен в общий список');
		activeDevelopers(ctx.project.id).forEach(m => notify(m.userId, ctx.project.id, `Новый свободный тикет ${ctx.ticket.number} в общем списке`));
		Store.persist();
		return { ok: true };
	};

	const takeInWork = ticketId => {
		const ctx = context(ticketId);
		if (ctx.error) return { ok: false, error: ctx.error };
		const me = Store.currentUser();
		ctx.ticket.assigneeId = me.id;
		ctx.ticket.status = 'В работе';
		log(ticketId, `Взят в работу: ${me.name}`);
		Store.persist();
		return { ok: true };
	};

	return { assign, sendToCommonList, takeInWork };
})();
