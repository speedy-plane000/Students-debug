const Store = (() => {
	const DB_KEY = 'kontur.db.v2';
	const SESSION_KEY = 'kontur.session.v2';
	const ROLES = { manager: 'Руководитель', developer: 'Разработчик', tester: 'Тестировщик' };
	const PRIORITIES = ['Низкий', 'Средний', 'Высокий'];
	const MAX_FILE_SIZE = 20 * 1024 * 1024;
	const DOC_EXTENSIONS = ['pdf', 'doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx', 'txt', 'log', 'csv', 'md', 'rtf', 'odt', 'ods', 'json'];
	const REPO_PATTERN = /^https:\/\/(www\.)?(github\.com|gitlab\.com|bitbucket\.org)\/[A-Za-z0-9_.-]+\/[A-Za-z0-9_.-]+?(\.git)?\/?$/;
	const TIME_PATTERN = /^([01]\d|2[0-3]):[0-5]\d$/;
	const DATE_PATTERN = /^\d{4}-\d{2}-\d{2}$/;
	const OPEN_RELEASE_STATUSES = ['Назначен', 'В работе', 'На доработке', 'Приостановлен'];

	const pad = n => String(n).padStart(2, '0');
	const toLocalIso = d => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
	const minutesFromNow = m => toLocalIso(new Date(Date.now() + m * 60000));
	const dayAt = (days, h, m) => {
		const d = new Date();
		d.setDate(d.getDate() + days);
		d.setHours(h, m, 0, 0);
		return toLocalIso(d);
	};
	let counter = 0;
	const uid = prefix => `${prefix}${Date.now().toString(36)}${(counter++).toString(36)}`;

	const seed = () => {
		const users = [
			{ id: 'u1', name: 'Марина Волкова', email: 'marina.volkova@example.ru' },
			{ id: 'u2', name: 'Анна Смирнова', email: 'anna.smirnova@example.ru' },
			{ id: 'u3', name: 'Алексей Орлов', email: 'alexey.orlov@example.ru' },
			{ id: 'u4', name: 'Денис Лебедев', email: 'denis.lebedev@example.ru' },
			{ id: 'u5', name: 'Ирина Белова', email: 'irina.belova@example.ru' },
			{ id: 'u6', name: 'Ольга Миронова', email: 'olga.mironova@example.ru' },
			{ id: 'u7', name: 'Павел Крылов', email: 'pavel.krylov@example.ru' }
		];
		const projects = [
			{ id: 'p1', code: 'PLT', name: 'Платформа', description: 'Личный кабинет и сервисы для клиентов', repo: 'https://github.com/team/platform', ownerId: 'u1', nextNumber: 144, updatedAt: minutesFromNow(-30) },
			{ id: 'p2', code: 'MOB', name: 'Мобильное приложение', description: 'Приложение для клиентов на iOS и Android', repo: 'https://github.com/team/mobile', ownerId: 'u4', nextNumber: 42, updatedAt: dayAt(-1, 16, 40) },
			{ id: 'p3', code: 'INT', name: 'Внутренний портал', description: 'Сервисы и инструменты для сотрудников', repo: 'https://github.com/team/internal', ownerId: 'u1', nextNumber: 1, updatedAt: dayAt(-3, 11, 15) },
			{ id: 'p4', code: 'ADM', name: 'Админ-панель', description: 'Инструменты поддержки и модерации', repo: 'https://github.com/team/admin', ownerId: 'u4', nextNumber: 1, updatedAt: dayAt(-5, 9, 0) }
		];
		const member = (projectId, userId, role, status = 'active') => ({ projectId, userId, role, status });
		const members = [
			member('p1', 'u1', 'manager'), member('p1', 'u2', 'tester'), member('p1', 'u3', 'developer'),
			member('p1', 'u4', 'developer'), member('p1', 'u5', 'tester'), member('p1', 'u6', 'tester', 'invited'),
			member('p2', 'u4', 'manager'), member('p2', 'u1', 'tester'), member('p2', 'u3', 'developer'), member('p2', 'u2', 'tester'),
			member('p3', 'u1', 'manager'), member('p3', 'u2', 'tester'), member('p3', 'u3', 'developer'), member('p3', 'u5', 'developer'), member('p3', 'u6', 'tester'),
			member('p4', 'u4', 'manager')
		];
		const invitations = [
			{ code: 'PRJ-7K4M-9X2P', projectId: 'p1', email: 'olga.mironova@example.ru', used: false },
			{ code: 'PRJ-4D8K-2M7Q', projectId: 'p4', email: 'marina.volkova@example.ru', used: false }
		];
		const ticket = (id, projectId, number, title, status, priority, authorId, assigneeId, startAt, dueAt, extra = {}) => ({
			id, projectId, number, title, status, priority, authorId, assigneeId,
			description: extra.description || 'При сохранении профиля с новым номером телефона сервер возвращает ошибку 500. Изменения не сохраняются.',
			steps: extra.steps || '1. Открыть настройки профиля.\n2. Изменить номер телефона.\n3. Нажать «Сохранить».',
			expected: extra.expected || 'Данные профиля сохранены',
			actual: extra.actual || 'Ошибка 500, данные не изменены',
			attachments: extra.attachments || [],
			moveRequest: extra.moveRequest || null,
			startAt, dueAt, createdAt: extra.createdAt || startAt
		});
		const png = [{ name: 'profile-error.png', size: 248 * 1024, type: 'image/png' }];
		const tickets = [
			ticket('t142', 'p1', 'PLT-142', 'Ошибка сохранения профиля', 'На рассмотрении', 'Высокий', 'u2', null, dayAt(0, 12, 0), dayAt(1, 18, 0), { attachments: png, createdAt: minutesFromNow(-2) }),
			ticket('t143', 'p1', 'PLT-143', 'Не обновляется аватар после загрузки', 'На рассмотрении', 'Средний', 'u2', null, dayAt(0, 12, 0), dayAt(2, 18, 0), { createdAt: minutesFromNow(-40) }),
			ticket('t140', 'p1', 'PLT-140', 'Ошибка сортировки по дате создания', 'На рассмотрении', 'Средний', 'u2', null, dayAt(0, 12, 0), dayAt(3, 12, 0), { createdAt: minutesFromNow(-90) }),
			ticket('t139', 'p1', 'PLT-139', 'Не отображается подсказка в настройках', 'На рассмотрении', 'Низкий', 'u2', null, dayAt(0, 12, 0), dayAt(3, 18, 0), { createdAt: minutesFromNow(-180) }),
			ticket('t137', 'p1', 'PLT-137', 'Не сохраняется аватар', 'На доработке', 'Высокий', 'u2', 'u3', dayAt(-2, 12, 0), dayAt(1, 18, 0), {
				createdAt: dayAt(-2, 12, 0),
				moveRequest: { requestedDueAt: dayAt(3, 18, 0), reason: 'нужна правка в смежном сервисе.' }
			}),
			ticket('t41', 'p2', 'MOB-41', 'Сброс фильтра при обновлении списка', 'В работе', 'Средний', 'u1', 'u4', dayAt(-3, 10, 0), dayAt(2, 12, 0)),
			ticket('t38', 'p2', 'MOB-38', 'Не приходит письмо с приглашением', 'На проверке', 'Высокий', 'u1', 'u3', dayAt(-4, 10, 0), dayAt(-1, 17, 0)),
			ticket('t36', 'p2', 'MOB-36', 'Смещение кнопки в форме проекта', 'В общем списке', 'Низкий', 'u1', null, dayAt(-2, 10, 0), dayAt(3, 18, 0)),
			ticket('t33', 'p2', 'MOB-33', 'Повторная загрузка вложения', 'Приостановлен', 'Средний', 'u1', 'u3', dayAt(-5, 10, 0), dayAt(2, 18, 0)),
			ticket('t29', 'p2', 'MOB-29', 'Некорректная дата в карточке', 'Завершён', 'Низкий', 'u1', 'u4', dayAt(-8, 10, 0), dayAt(-4, 14, 0)),
			ticket('t37', 'p2', 'MOB-37', 'Не сохраняется аватар', 'На доработке', 'Высокий', 'u1', 'u3', dayAt(-3, 10, 0), dayAt(1, 18, 0))
		];
		const comments = [
			{ id: 'c1', ticketId: 't142', authorId: 'u2', text: 'Повторяется в Chrome 130 и Firefox 132. Скриншот во вложении.', createdAt: minutesFromNow(-2) }
		];
		const history = tickets.map(t => ({ id: uid('h'), ticketId: t.id, text: 'Создан и отправлен руководителю', at: t.createdAt }));
		const notifications = [
			{ id: 'n1', userId: 'u1', projectId: 'p1', text: 'Анна Смирнова создала тикет PLT-142', link: 'ticket.html?id=t142', at: minutesFromNow(0), read: false },
			{ id: 'n2', userId: 'u1', projectId: 'p1', text: 'Алексей Орлов отправил PLT-138 на проверку', link: 'tickets.html', at: minutesFromNow(-5), read: false },
			{ id: 'n3', userId: 'u1', projectId: 'p1', text: 'Ольга Миронова вошла в проект', link: 'members.html', at: minutesFromNow(-12), read: false }
		];
		return { users, projects, members, invitations, tickets, comments, history, notifications };
	};

	let cache = null;
	const persist = () => localStorage.setItem(DB_KEY, JSON.stringify(cache));
	const db = () => {
		if (cache) return cache;
		const raw = localStorage.getItem(DB_KEY);
		cache = raw ? JSON.parse(raw) : seed();
		if (!raw) persist();
		return cache;
	};
	const reset = () => {
		localStorage.removeItem(DB_KEY);
		localStorage.removeItem(SESSION_KEY);
		cache = null;
	};

	const getSession = () => JSON.parse(localStorage.getItem(SESSION_KEY) || '{"userId":"u1","projectId":null}');
	const setSession = patch => localStorage.setItem(SESSION_KEY, JSON.stringify({ ...getSession(), ...patch }));
	const getProjectId = () => getSession().projectId;
	const setProjectId = id => setSession({ projectId: id });

	const user = id => db().users.find(u => u.id === id) || null;
	const userByEmail = email => db().users.find(u => u.email.toLowerCase() === String(email).trim().toLowerCase()) || null;
	const currentUser = () => user(getSession().userId);
	const project = id => db().projects.find(p => p.id === id) || null;
	const membership = (projectId, userId = getSession().userId) => db().members.find(m => m.projectId === projectId && m.userId === userId) || null;
	const touch = projectId => {
		const p = project(projectId);
		if (p) p.updatedAt = toLocalIso(new Date());
	};

	const notify = (userId, projectId, text, link) => {
		db().notifications.unshift({ id: uid('n'), userId, projectId, text, link, at: toLocalIso(new Date()), read: false });
	};
	const notifyManagers = (projectId, text, link, exceptUserId) => {
		db().members
			.filter(m => m.projectId === projectId && m.role === 'manager' && m.status === 'active' && m.userId !== exceptUserId)
			.forEach(m => notify(m.userId, projectId, text, link));
	};
	const notifications = () => db().notifications.filter(n => n.userId === getSession().userId).sort((a, b) => b.at.localeCompare(a.at));
	const markAllRead = () => {
		db().notifications.forEach(n => {
			if (n.userId === getSession().userId) n.read = true;
		});
		persist();
	};
	const markRead = id => {
		const n = db().notifications.find(x => x.id === id);
		if (n) n.read = true;
		persist();
	};

	const checkRepository = url => {
		const value = String(url || '').trim();
		return REPO_PATTERN.test(value) ? { ok: true } : { ok: false };
	};
	const repoLabel = url => {
		const parts = String(url || '').replace(/\.git$/, '').replace(/\/$/, '').split('/');
		return parts.length >= 2 ? `${parts[parts.length - 2]}/${parts[parts.length - 1]}` : '';
	};

	const openTicketsCount = projectId => db().tickets.filter(t => t.projectId === projectId && t.status !== 'Завершён').length;
	const projectCards = () => {
		const userId = getSession().userId;
		return db().members
			.filter(m => m.userId === userId)
			.map(m => ({
				project: project(m.projectId),
				member: m,
				openTickets: openTicketsCount(m.projectId),
				membersCount: db().members.filter(x => x.projectId === m.projectId).length
			}))
			.sort((a, b) => b.project.updatedAt.localeCompare(a.project.updatedAt));
	};
	const managedProjects = () => projectCards().filter(c => c.member.role === 'manager' && c.member.status === 'active').map(c => c.project);
	const testerProjects = () => projectCards().filter(c => c.member.role === 'tester' && c.member.status === 'active').map(c => c.project);

	const validateProject = (data, ignoreId) => {
		const errors = {};
		const name = String(data.name || '').trim();
		const code = String(data.code || '').trim().toUpperCase();
		const repo = String(data.repo || '').trim();
		if (!name) errors.name = 'Введите название проекта';
		else if (name.length > 80) errors.name = 'Название не длиннее 80 символов';
		if (!code) errors.code = 'Введите код проекта';
		else if (!/^[A-Z0-9]{2,6}$/.test(code)) errors.code = 'Код: от 2 до 6 латинских букв или цифр';
		else if (db().projects.some(p => p.code === code && p.id !== ignoreId)) errors.code = 'Такой код проекта уже занят';
		if (repo && !checkRepository(repo).ok) errors.repo = 'Репозиторий не найден или недоступен. Проверьте адрес и права доступа.';
		return errors;
	};
	const createProject = data => {
		const errors = validateProject(data);
		if (Object.keys(errors).length) return { ok: false, errors };
		const owner = currentUser();
		const created = {
			id: uid('p'),
			code: String(data.code).trim().toUpperCase(),
			name: String(data.name).trim(),
			description: String(data.description || '').trim(),
			repo: String(data.repo || '').trim(),
			ownerId: owner.id,
			nextNumber: 1,
			updatedAt: toLocalIso(new Date())
		};
		db().projects.push(created);
		db().members.push({ projectId: created.id, userId: owner.id, role: 'manager', status: 'active' });
		persist();
		return { ok: true, project: created };
	};
	const updateProject = (id, data) => {
		const errors = validateProject(data, id);
		if (Object.keys(errors).length) return { ok: false, errors };
		const p = project(id);
		p.name = String(data.name).trim();
		p.code = String(data.code).trim().toUpperCase();
		p.description = String(data.description || '').trim();
		p.repo = String(data.repo || '').trim();
		touch(id);
		persist();
		return { ok: true, project: p };
	};
	const deleteProject = (id, confirmName) => {
		const p = project(id);
		if (!p || String(confirmName).trim() !== p.name) return { ok: false };
		const ticketIds = db().tickets.filter(t => t.projectId === id).map(t => t.id);
		db().projects = db().projects.filter(x => x.id !== id);
		db().members = db().members.filter(m => m.projectId !== id);
		db().invitations = db().invitations.filter(i => i.projectId !== id);
		db().tickets = db().tickets.filter(t => t.projectId !== id);
		db().comments = db().comments.filter(c => !ticketIds.includes(c.ticketId));
		db().history = db().history.filter(h => !ticketIds.includes(h.ticketId));
		db().notifications = db().notifications.filter(n => n.projectId !== id);
		if (getProjectId() === id) setProjectId(null);
		persist();
		return { ok: true };
	};

	const joinByCode = code => {
		const value = String(code || '').trim().toUpperCase();
		const current = currentUser();
		const invitation = db().invitations.find(i => i.code === value && !i.used && i.email.toLowerCase() === current.email.toLowerCase());
		if (!invitation || !project(invitation.projectId)) return { ok: false };
		invitation.used = true;
		let m = membership(invitation.projectId, current.id);
		if (m) {
			m.status = 'active';
		} else {
			m = { projectId: invitation.projectId, userId: current.id, role: null, status: 'active' };
			db().members.push(m);
		}
		const p = project(invitation.projectId);
		notifyManagers(p.id, `${current.name} присоединился(-ась) к проекту «${p.name}»`, 'members.html', current.id);
		touch(p.id);
		persist();
		return { ok: true, project: p };
	};

	const members = projectId => db().members
		.filter(m => m.projectId === projectId)
		.map(m => ({ ...m, user: user(m.userId), invitation: db().invitations.find(i => i.projectId === projectId && i.email.toLowerCase() === user(m.userId).email.toLowerCase() && !i.used) || null }))
		.sort((a, b) => (a.role === 'manager' ? 0 : 1) - (b.role === 'manager' ? 0 : 1));
	const invite = (projectId, email) => {
		const value = String(email || '').trim();
		if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(value)) return { ok: false, error: 'Введите корректный email' };
		const account = userByEmail(value);
		if (!account) return { ok: false, error: 'Аккаунт с таким email не найден' };
		if (membership(projectId, account.id)) return { ok: false, error: 'Этот человек уже участвует в проекте' };
		const invitation = {
			code: `PRJ-${Math.random().toString(36).slice(2, 6).toUpperCase()}-${Math.random().toString(36).slice(2, 6).toUpperCase()}`,
			projectId, email: account.email, used: false
		};
		db().invitations.push(invitation);
		db().members.push({ projectId, userId: account.id, role: null, status: 'invited' });
		touch(projectId);
		persist();
		return { ok: true, invitation, account };
	};
	const releaseTickets = (projectId, userId) => {
		db().tickets.forEach(t => {
			if (t.projectId === projectId && t.assigneeId === userId && OPEN_RELEASE_STATUSES.includes(t.status)) {
				t.assigneeId = null;
				t.status = 'В общем списке';
				db().history.push({ id: uid('h'), ticketId: t.id, text: 'Исполнитель снят, тикет возвращён в общий список', at: toLocalIso(new Date()) });
			}
		});
	};
	const setRole = (projectId, userId, role) => {
		const p = project(projectId);
		const m = membership(projectId, userId);
		if (!m || !ROLES[role]) return { ok: false, error: 'Недопустимая роль' };
		if (p.ownerId === userId) return { ok: false, error: 'Роль владельца проекта изменить нельзя' };
		if (m.role === 'developer' && role !== 'developer') releaseTickets(projectId, userId);
		m.role = role;
		notify(userId, projectId, `Вам назначена роль «${ROLES[role]}» в проекте «${p.name}»`, null);
		touch(projectId);
		persist();
		return { ok: true };
	};
	const removeMember = (projectId, userId) => {
		const p = project(projectId);
		if (p.ownerId === userId) return { ok: false, error: 'Владельца проекта удалить нельзя' };
		const account = user(userId);
		releaseTickets(projectId, userId);
		db().members = db().members.filter(m => !(m.projectId === projectId && m.userId === userId));
		db().invitations = db().invitations.filter(i => !(i.projectId === projectId && i.email.toLowerCase() === account.email.toLowerCase() && !i.used));
		touch(projectId);
		persist();
		return { ok: true };
	};

	const isValidTime = v => TIME_PATTERN.test(String(v || ''));
	const isValidDate = v => {
		if (!DATE_PATTERN.test(String(v || ''))) return false;
		const d = new Date(`${v}T00:00`);
		return !Number.isNaN(d.getTime()) && toLocalIso(d).slice(0, 10) === v;
	};
	const validateTicket = (data, options = {}) => {
		const errors = {};
		const required = { title: 'Введите название', description: 'Введите описание', steps: 'Опишите шаги воспроизведения' };
		Object.keys(required).forEach(key => {
			if (!String(data[key] || '').trim()) errors[key] = required[key];
		});
		if (!data.projectId || !project(data.projectId)) errors.projectId = 'Выберите проект';
		if (!PRIORITIES.includes(data.priority)) errors.priority = 'Выберите приоритет';
		if (!isValidDate(data.startDate)) errors.startDate = 'Укажите дату начала';
		if (!isValidTime(data.startTime)) errors.startTime = 'Введите время от 00:00 до 23:59.';
		if (!isValidDate(data.dueDate)) errors.dueDate = 'Укажите срок выполнения';
		if (!isValidTime(data.dueTime)) errors.dueTime = 'Введите время от 00:00 до 23:59.';
		if (!errors.startDate && !errors.startTime && !errors.dueDate && !errors.dueTime) {
			const start = `${data.startDate}T${data.startTime}`;
			const due = `${data.dueDate}T${data.dueTime}`;
			if (due < start) {
				errors.dueDate = 'Срок не может быть раньше даты начала.';
			} else if (due < toLocalIso(new Date()) && !options.allowPastDue) {
				errors.dueDate = 'Срок не может быть в прошлом.';
			}
		}
		return errors;
	};
	const validateFile = file => {
		const ext = String(file.name).split('.').pop().toLowerCase();
		const typeOk = String(file.type).startsWith('image/') || String(file.type).startsWith('video/') || DOC_EXTENSIONS.includes(ext);
		if (!typeOk) return 'Допустимы изображения, документы и видео.';
		if (file.size > MAX_FILE_SIZE) return `Размер ${Math.ceil(file.size / 1048576)} МБ превышает ограничение 20 МБ. Удалите файл или выберите другой.`;
		return null;
	};

	const addHistory = (ticketId, text) => db().history.push({ id: uid('h'), ticketId, text, at: toLocalIso(new Date()) });
	const createTicket = (data, attachments) => {
		const errors = validateTicket(data);
		const m = membership(data.projectId);
		if (!errors.projectId && (!m || m.role !== 'tester' || m.status !== 'active')) errors.projectId = 'Создавать тикеты в этом проекте может только тестировщик';
		if (Object.keys(errors).length) return { ok: false, errors };
		const author = currentUser();
		const p = project(data.projectId);
		const number = `${p.code}-${p.nextNumber}`;
		p.nextNumber += 1;
		const created = {
			id: uid('t'), projectId: p.id, number,
			title: data.title.trim(), description: data.description.trim(), steps: data.steps.trim(),
			expected: String(data.expected || '').trim(), actual: String(data.actual || '').trim(),
			priority: data.priority, status: 'На рассмотрении',
			authorId: author.id, assigneeId: null,
			attachments: attachments.map(a => ({ name: a.name, size: a.size, type: a.type })),
			moveRequest: null,
			startAt: `${data.startDate}T${data.startTime}`, dueAt: `${data.dueDate}T${data.dueTime}`,
			createdAt: toLocalIso(new Date())
		};
		db().tickets.push(created);
		if (String(data.comment || '').trim()) {
			db().comments.push({ id: uid('c'), ticketId: created.id, authorId: author.id, text: data.comment.trim(), createdAt: created.createdAt });
		}
		addHistory(created.id, 'Создан и отправлен руководителю');
		notifyManagers(p.id, `${author.name} создал(а) тикет ${number}`, `ticket.html?id=${created.id}`, author.id);
		touch(p.id);
		persist();
		return { ok: true, ticket: created };
	};
	const tickets = projectId => db().tickets.filter(t => t.projectId === projectId).sort((a, b) => b.createdAt.localeCompare(a.createdAt));
	const ticket = id => db().tickets.find(t => t.id === id) || null;
	const ticketComments = id => db().comments.filter(c => c.ticketId === id).sort((a, b) => a.createdAt.localeCompare(b.createdAt));
	const ticketHistory = id => db().history.filter(h => h.ticketId === id).sort((a, b) => b.at.localeCompare(a.at));
	const addComment = (ticketId, text) => {
		const value = String(text || '').trim();
		if (!value) return { ok: false };
		db().comments.push({ id: uid('c'), ticketId, authorId: currentUser().id, text: value, createdAt: toLocalIso(new Date()) });
		persist();
		return { ok: true };
	};
	const updateTicket = (id, data, attachments) => {
		const t = ticket(id);
		const errors = validateTicket(data, { allowPastDue: true });
		if (!errors.projectId && !managedProjects().some(p => p.id === data.projectId)) errors.projectId = 'Выбрать можно только проект, где вы руководитель';
		if (Object.keys(errors).length) return { ok: false, errors };
		const changes = [];
		if (t.projectId !== data.projectId) {
			const p = project(data.projectId);
			t.projectId = p.id;
			t.number = `${p.code}-${p.nextNumber}`;
			p.nextNumber += 1;
			t.assigneeId = null;
			t.status = 'На рассмотрении';
			t.moveRequest = null;
			changes.push('Проект изменён');
		}
		const next = {
			title: data.title.trim(), description: data.description.trim(), steps: data.steps.trim(),
			expected: String(data.expected || '').trim(), actual: String(data.actual || '').trim(), priority: data.priority,
			startAt: `${data.startDate}T${data.startTime}`, dueAt: `${data.dueDate}T${data.dueTime}`
		};
		Object.keys(next).forEach(key => {
			if (t[key] !== next[key]) {
				changes.push('Изменены детали тикета');
				t[key] = next[key];
			}
		});
		t.attachments = attachments.map(a => ({ name: a.name, size: a.size, type: a.type }));
		if (String(data.comment || '').trim()) addComment(id, data.comment);
		addHistory(id, changes.length ? [...new Set(changes)].join(', ') : 'Тикет сохранён без изменений');
		touch(t.projectId);
		persist();
		return { ok: true, ticket: t };
	};
	const deleteTicket = id => {
		const t = ticket(id);
		if (!t || t.status !== 'На рассмотрении') return { ok: false };
		db().tickets = db().tickets.filter(x => x.id !== id);
		db().comments = db().comments.filter(c => c.ticketId !== id);
		db().history = db().history.filter(h => h.ticketId !== id);
		notify(t.authorId, t.projectId, `Тикет ${t.number} удалён руководителем`, null);
		persist();
		return { ok: true };
	};
	const assignTicket = (id, assigneeId) => {
		const t = ticket(id);
		if (assigneeId) {
			t.assigneeId = assigneeId;
			t.status = 'Назначен';
			addHistory(id, `Назначен исполнитель: ${user(assigneeId).name}`);
			notify(assigneeId, t.projectId, `Вам назначен тикет ${t.number}`, null);
		} else {
			t.assigneeId = null;
			t.status = 'В общем списке';
			addHistory(id, 'Тикет отправлен в общий список');
		}
		persist();
	};
	const takeTicket = id => {
		const t = ticket(id);
		t.assigneeId = currentUser().id;
		t.status = 'В работе';
		addHistory(id, `Взят в работу: ${currentUser().name}`);
		persist();
	};
	const resolveMoveRequest = (id, decision, dueAt) => {
		const t = ticket(id);
		if (!t.moveRequest) return;
		if (decision === 'agree') {
			t.dueAt = t.moveRequest.requestedDueAt;
			addHistory(id, 'Перенос срока согласован');
		} else if (decision === 'custom') {
			t.dueAt = dueAt;
			addHistory(id, 'Руководитель предложил свою дату срока');
		} else {
			addHistory(id, 'Перенос срока отклонён');
		}
		t.moveRequest = null;
		persist();
	};
	const developers = projectId => members(projectId).filter(m => m.role === 'developer' && m.status === 'active').map(m => ({
		user: m.user,
		openCount: db().tickets.filter(t => t.assigneeId === m.userId && t.status !== 'Завершён').length
	}));

	return {
		ROLES, PRIORITIES, MAX_FILE_SIZE, db, reset, toLocalIso,
		getProjectId, setProjectId, user, currentUser, project, membership,
		projectCards, managedProjects, testerProjects, repoLabel, checkRepository,
		validateProject, createProject, updateProject, deleteProject, joinByCode,
		members, invite, setRole, removeMember,
		validateTicket, validateFile, createTicket, tickets, ticket, ticketComments, ticketHistory,
		addComment, updateTicket, deleteTicket, assignTicket, takeTicket, resolveMoveRequest, developers,
		notifications, markAllRead, markRead
	};
})();
