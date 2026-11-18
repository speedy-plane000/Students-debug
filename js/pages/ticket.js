const ctx = Shell.init({ roles: ['manager'], active: 'tickets', crumbs: project => [project.name, ['Входящие', 'tickets.html'], 'Тикет'] });
const found = ctx ? Store.ticket(ctx.params.get('id')) : null;
if (ctx && (!found || found.projectId !== ctx.project.id)) location.replace('tickets.html');
else if (ctx) {
	const { $, esc, icon } = UI;
	const id = found.id;

	const render = () => {
		const t = Store.ticket(id);
		const author = Store.user(t.authorId);
		const assignee = t.assigneeId ? Store.user(t.assigneeId).name : 'Не назначен';
		const review = t.status === 'На рассмотрении';

		document.title = `${t.number} ${t.title} — Контур`;
		document.querySelector('.crumbs').innerHTML = `${esc(ctx.project.name)} / <a href="tickets.html">Входящие</a> / ${esc(t.number)}`;
		$('#t-title').textContent = t.title;
		$('#t-sub').textContent = `${t.number} · ${review ? 'Входящие / ' : ''}${t.status}`;

		const editable = !['На проверке', 'Завершён'].includes(t.status);
		$('#t-actions').innerHTML = [
			'<a class="btn" href="tickets.html">Отмена</a>',
			editable ? `<a class="btn" href="ticket-edit.html?id=${t.id}">${icon('pencil')}Изменить</a>` : '',
			review ? `<button class="btn" type="button" data-open="modal-delete-ticket">${icon('trash')}Удалить</button>` : '',
			review ? `<button class="btn btn-primary" id="take-work" type="button">${icon('play')}Взять ${esc(t.number)} в работу</button>` : '',
			review ? '<button class="btn btn-primary" type="button" data-open="modal-assign">Назначить разработчику</button>' : ''
		].join('');

		const request = $('#request');
		request.classList.toggle('hidden', !t.moveRequest);
		if (t.moveRequest) {
			request.innerHTML = `<div class="lbl">Запрос на перенос срока</div>
				<p>${esc(assignee)} просит перенести срок с ${UI.fmtFull(t.dueAt).replace(' в ', ' ')} на ${UI.fmtFull(t.moveRequest.requestedDueAt).replace(' в ', ' ')}. Обоснование: ${esc(t.moveRequest.reason)}</p>
				<div class="btns"><button class="btn btn-primary btn-sm" id="req-agree" type="button">Согласиться</button><button class="btn btn-sm" id="req-refuse" type="button">Отказать</button><button class="btn btn-sm" type="button" data-open="modal-own-date">Предложить свою дату</button></div>`;
		}

		const attachments = t.attachments.length
			? t.attachments.map(f => `<div class="file"><span class="ico">${icon(String(f.type).startsWith('image/') ? 'img' : 'file')}</span><div><b>${esc(f.name)}</b><small>${UI.fmtSize(f.size)} · Файл загружен</small></div><button class="act" type="button" aria-label="Скачать">${icon('download')}</button></div>`).join('')
			: '<p class="comment-empty" style="margin:0">Вложений нет</p>';
		$('#t-main').innerHTML = `<div class="badges">${UI.statusChip(t.status)}<span class="chip chip-danger">${esc(t.priority)} приоритет</span></div>
			<h3 class="sec">Описание</h3>
			<p class="desc">${esc(t.description)}</p>
			<div class="two-col">
				<div><b class="t">Шаги воспроизведения</b>${esc(t.steps).replace(/\n/g, '<br>')}</div>
				<div><div class="kv">Ожидаемый результат</div><div class="kv-v">${esc(t.expected) || '—'}</div><div class="kv">Фактический результат</div><div class="kv-v" style="margin:0">${esc(t.actual) || '—'}</div></div>
			</div>
			<div class="attach-title">Вложения</div>${attachments}`;

		$('#t-props').innerHTML = `<h2>Свойства</h2>
			<div class="kv">Проект</div><div class="kv-v">${esc(ctx.project.name)}</div>
			<div class="kv">Автор</div><div class="kv-v">${esc(author.name)} · Тестировщик</div>
			<div class="kv">Исполнитель</div><div class="kv-v">${esc(assignee)}</div>
			<div class="kv">Руководитель</div><div class="kv-v">${esc(Store.user(ctx.project.ownerId).name)}</div>
			<div class="sep"></div>
			<div class="kv">Создан</div><div class="kv-v">${UI.fmtFull(t.createdAt)}</div>
			<div class="kv">Плановое начало</div><div class="kv-v">${UI.fmtFull(t.startAt)}</div>
			<div class="kv">Срок выполнения</div><div class="kv-v" style="margin:0">${UI.fmtFull(t.dueAt)}</div>`;

		const history = Store.ticketHistory(id);
		$('#t-history').innerHTML = history.map(h => `<div class="hist" style="margin-bottom:10px">${UI.fmtShort(h.at)} · ${esc(h.text)}</div>`).join('');

		const comments = Store.ticketComments(id);
		$('#comments-title').textContent = `Комментарии · ${comments.length}`;
		$('#comments').innerHTML = comments.length
			? comments.map(c => {
				const a = Store.user(c.authorId);
				return `<div class="comment"><span class="avatar">${UI.initials(a.name)}</span><div><b>${esc(a.name)}</b><time>${UI.fmtShort(c.createdAt)}</time><p>${esc(c.text)}</p></div></div>`;
			}).join('')
			: '<p class="comment-empty">Комментариев пока нет</p>';
	};

	const fillAssign = () => {
		const t = Store.ticket(id);
		const developers = Store.developers(ctx.project.id);
		$('#assign-lead').textContent = `${t.number} · ${t.title}`;
		$('#assign-list').innerHTML = developers.length
			? developers.map((d, i) => `<button class="pick${i === 0 ? ' selected' : ''}" type="button" data-user="${d.user.id}" data-name="${esc(d.user.name.split(' ')[0])}"><span class="avatar">${UI.initials(d.user.name)}</span><span><b>${esc(d.user.name)}</b><small>${d.openCount} ${UI.plural(d.openCount, ['тикет', 'тикета', 'тикетов'])} в работе</small></span><span class="radio">${icon('check')}</span></button>`).join('')
			: '<p class="comment-empty">В проекте нет разработчиков. Назначьте роль участникам или отправьте тикет в общий список.</p>';
		$('#assign-name').textContent = developers.length ? developers[0].user.name.split(' ')[0] : 'разработчика';
		$('#assign-confirm').disabled = !developers.length;
	};

	document.addEventListener('click', e => {
		const pick = e.target.closest('#assign-list .pick');
		if (pick) {
			document.querySelectorAll('#assign-list .pick').forEach(p => p.classList.remove('selected'));
			pick.classList.add('selected');
			$('#assign-name').textContent = pick.dataset.name;
			return;
		}
		if (e.target.closest('#req-agree')) {
			Store.resolveMoveRequest(id, 'agree');
			render();
			UI.toast('Перенос срока согласован');
		} else if (e.target.closest('#req-refuse')) {
			Store.resolveMoveRequest(id, 'refuse');
			render();
			UI.toast('Запрос на перенос срока отклонён');
		} else if (e.target.closest('#take-work')) {
			Store.takeTicket(id);
			render();
			UI.toast('Тикет взят в работу');
		}
	});

	$('#assign-confirm').addEventListener('click', () => {
		const selected = $('#assign-list .pick.selected');
		if (!selected) return;
		Store.assignTicket(id, selected.dataset.user);
		UI.flash('Тикет назначен разработчику');
		location.href = 'tickets.html';
	});
	$('#assign-common').addEventListener('click', () => {
		Store.assignTicket(id, null);
		UI.flash('Тикет отправлен в общий список');
		location.href = 'tickets.html';
	});
	$('#delete-confirm').addEventListener('click', () => {
		const result = Store.deleteTicket(id);
		UI.flash(result.ok ? 'Тикет удалён' : 'Удалить можно только тикет «На рассмотрении»');
		location.href = 'tickets.html';
	});

	$('#comment-form').addEventListener('submit', e => {
		e.preventDefault();
		const input = $('#comment-input');
		if (!Store.addComment(id, input.value).ok) return;
		input.value = '';
		render();
	});

	const ownDate = $('#own-date');
	const ownTime = $('#own-time');
	document.addEventListener('click', e => {
		if (!e.target.closest('[data-open="modal-own-date"]')) return;
		const t = Store.ticket(id);
		$('#own-lead').textContent = `Исполнитель просил ${UI.fmtFull(t.moveRequest.requestedDueAt).replace(' в ', ' ')}. Укажите свой срок — он сразу станет сроком тикета.`;
		ownDate.value = t.moveRequest.requestedDueAt.slice(0, 10);
		ownTime.value = t.moveRequest.requestedDueAt.slice(11, 16);
		UI.setFieldError($('#own-date-field'), '');
		UI.setFieldError($('#own-time-field'), '');
	});
	$('#own-submit').addEventListener('click', () => {
		const t = Store.ticket(id);
		const errors = Store.validateTicket({
			title: 'x', description: 'x', steps: 'x', projectId: t.projectId, priority: t.priority,
			startDate: t.startAt.slice(0, 10), startTime: t.startAt.slice(11, 16), dueDate: ownDate.value, dueTime: ownTime.value
		});
		UI.setFieldError($('#own-date-field'), errors.dueDate || '');
		UI.setFieldError($('#own-time-field'), errors.dueTime || '');
		if (errors.dueDate || errors.dueTime) return;
		Store.resolveMoveRequest(id, 'custom', `${ownDate.value}T${ownTime.value}`);
		UI.closeModals();
		render();
		UI.toast('Предложена своя дата срока');
	});

	render();
	fillAssign();
}
