const ctx = Shell.init({ roles: ['manager'], active: 'members', crumbs: project => [project.name, 'Участники'] });
if (ctx) {
	const { $, $$, esc } = UI;
	const project = ctx.project;
	const ROLE_INFO = {
		manager: 'Управляет проектом, участниками и тикетами. Может брать тикеты в работу.',
		developer: 'Сможет брать тикеты в работу, оставлять комментарии и отправлять исправления на проверку.',
		tester: 'Создаёт тикеты и проверяет результат исправления.'
	};
	let target = null;

	$('#invite-lead').textContent = `Участник получит письмо с кодом для входа в проект «${project.name}».`;
	$('#members-note').textContent = `Роль определяет доступные действия с тикетами. Изменения применяются только к проекту «${project.name}».`;

	const row = m => {
		const isOwner = m.userId === project.ownerId;
		const isMe = m.userId === ctx.user.id;
		const role = m.role ? Store.ROLES[m.role] : '<span class="role-none">Роль не назначена</span>';
		const status = m.status === 'active' ? '<span class="chip chip-ok">Активен</span>' : '<span class="chip chip-warn">Приглашён</span>';
		const actions = isOwner
			? '<span>Владелец проекта</span>'
			: `<button type="button" data-act="role" data-user="${m.userId}">Изменить роль</button><button class="del" type="button" data-act="remove" data-user="${m.userId}">Удалить</button>`;
		return `<div class="t-row"><div class="person"><span class="avatar lg">${UI.initials(m.user.name)}</span><div><b>${esc(m.user.name)}${isMe ? ' · Вы' : ''}</b><small>${esc(m.user.email)}</small></div></div><div class="role-cell">${role}</div><div>${status}</div><div class="act-links">${actions}</div></div>`;
	};

	const render = () => {
		const list = Store.members(project.id);
		$('#members-sub').textContent = `${project.name} · ${list.length} ${UI.plural(list.length, ['участник', 'участника', 'участников'])} · Управление доступом и ролями`;
		$('#members-body').innerHTML = list.map(row).join('');
	};

	const email = $('#invite-email');
	const emailField = $('#invite-email-field');
	const sent = $('#invite-sent');
	let inviteSent = false;
	const resetInvite = () => {
		inviteSent = false;
		sent.classList.add('hidden');
		email.value = '';
		email.readOnly = false;
		UI.setFieldError(emailField, '');
	};
	const sendInvite = () => {
		if (inviteSent) {
			UI.closeModals();
			return;
		}
		const result = Store.invite(project.id, email.value);
		if (!result.ok) {
			UI.setFieldError(emailField, result.error);
			return;
		}
		inviteSent = true;
		email.readOnly = true;
		$('#invite-sent-email').textContent = result.account.email;
		$('#invite-sent-code').textContent = `Код входа: ${result.invitation.code}`;
		sent.classList.remove('hidden');
		render();
	};
	$('#invite-open').addEventListener('click', resetInvite);
	$('#invite-done').addEventListener('click', sendInvite);
	email.addEventListener('input', () => UI.setFieldError(emailField, ''));
	email.addEventListener('keydown', e => {
		if (e.key === 'Enter') sendInvite();
	});
	$$('[data-close]', $('#modal-invite')).forEach(b => b.addEventListener('click', render));

	const roleSelect = $('#new-role');
	const syncRoleInfo = () => {
		$('#role-info-title').textContent = Store.ROLES[roleSelect.value];
		$('#role-info-text').textContent = ROLE_INFO[roleSelect.value];
	};
	roleSelect.addEventListener('change', syncRoleInfo);

	$('#members-body').addEventListener('click', e => {
		const button = e.target.closest('[data-act]');
		if (!button) return;
		target = Store.members(project.id).find(m => m.userId === button.dataset.user);
		if (button.dataset.act === 'role') {
			$('#role-lead').textContent = `Новая роль будет действовать только в проекте «${project.name}».`;
			$('#role-ava').textContent = UI.initials(target.user.name);
			$('#role-name').textContent = target.user.name;
			$('#role-email').textContent = target.user.email;
			$('#cur-role').value = target.role ? Store.ROLES[target.role] : 'Не назначена';
			roleSelect.value = target.role || 'tester';
			syncRoleInfo();
			UI.openModal('modal-role');
		} else {
			const openTickets = Store.tickets(project.id).filter(t => t.assigneeId === target.userId && t.status !== 'Завершён' && t.status !== 'На проверке').length;
			$('#rm-name').textContent = target.user.name;
			$('#rm-project').textContent = project.name;
			$('#rm-line').textContent = `${target.user.name} · ${target.user.email}`;
			$('#rm-role').textContent = target.role ? Store.ROLES[target.role] : 'Не назначена';
			$('#rm-alert-title').textContent = openTickets ? `Открытых назначенных тикетов: ${openTickets}` : 'Открытых назначенных тикетов нет';
			$('#rm-alert-text').textContent = openTickets ? 'Они вернутся в общий список. Доступ к другим проектам не изменится.' : 'Доступ к другим проектам не изменится.';
			UI.openModal('modal-remove');
		}
	});

	$('#role-save').addEventListener('click', () => {
		const result = Store.setRole(project.id, target.userId, roleSelect.value);
		UI.closeModals();
		render();
		UI.toast(result.ok ? `Роль сохранена: ${Store.ROLES[roleSelect.value]}` : result.error);
	});
	$('#remove-confirm').addEventListener('click', () => {
		const result = Store.removeMember(project.id, target.userId);
		UI.closeModals();
		render();
		UI.toast(result.ok ? 'Участник удалён из проекта' : result.error);
	});

	render();
}
