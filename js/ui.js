const UI = (() => {
	const FLASH_KEY = 'kontur.flash';
	const ICONS = {
		projects: '<path d="M3 7.5A2.5 2.5 0 0 1 5.5 5H9l2 2h7.5A2.5 2.5 0 0 1 21 9.5v8a2.5 2.5 0 0 1-2.5 2.5h-13A2.5 2.5 0 0 1 3 17.5z"/><path d="M8 16v-3M12 16v-5M16 16v-2"/>',
		tickets: '<rect x="3" y="6" width="18" height="12" rx="3"/><path d="M3 10.5h18"/><path d="M7 15h3"/>',
		users: '<circle cx="9" cy="8.5" r="3.2"/><path d="M3 19c0-3.2 2.7-5 6-5s6 1.8 6 5"/><path d="M16 5.5a3 3 0 0 1 0 6M18.5 14.5c1.5.7 2.5 2 2.5 4.5"/>',
		settings: '<path d="M4 8h8M17 8h3M4 16h3M12 16h8"/><circle cx="14.5" cy="8" r="2.2"/><circle cx="9.5" cy="16" r="2.2"/>',
		bell: '<path d="M6 16v-5a6 6 0 1 1 12 0v5l1.5 2h-15z"/><path d="M10 20.5a2 2 0 0 0 4 0"/>',
		search: '<circle cx="11" cy="11" r="6.5"/><path d="m20 20-4.2-4.2"/>',
		filter: '<path d="M4 7h16M7 12h10M10 17h4"/>',
		plus: '<path d="M12 5v14M5 12h14"/>',
		pencil: '<path d="m4 20 1-4L16.5 4.5a2 2 0 0 1 3 3L8 19z"/><path d="m14.5 6.5 3 3"/>',
		trash: '<path d="M4 7h16M10 11v6M14 11v6M6 7l1 12a2 2 0 0 0 2 2h6a2 2 0 0 0 2-2l1-12M9 7V4h6v3"/>',
		play: '<path d="M7 5v14l11-7z"/>',
		clip: '<path d="m20 11-8.5 8.5a5 5 0 0 1-7-7L13 4a3.3 3.3 0 0 1 5 5l-8.5 8.5a1.7 1.7 0 0 1-2.5-2.5L14.5 8"/>',
		send: '<path d="M21 3 10 14M21 3l-7 18-4-7-7-4z"/>',
		download: '<path d="M12 4v11M7 11l5 5 5-5M5 20h14"/>',
		check: '<circle cx="12" cy="12" r="9"/><path d="m8 12.3 2.8 2.8L16 9.5"/>',
		info: '<circle cx="12" cy="12" r="9"/><path d="M12 11v5M12 7.8v.1"/>',
		alert: '<circle cx="12" cy="12" r="9"/><path d="M12 7.5v5.5M12 16.3v.1"/>',
		x: '<path d="M6 6l12 12M18 6 6 18"/>',
		branch: '<circle cx="6" cy="18" r="2"/><circle cx="6" cy="6" r="2"/><circle cx="18" cy="9" r="2"/><path d="M6 8v8M18 11c0 4-6 3-11 6.5"/>',
		img: '<rect x="4" y="3" width="16" height="18" rx="3"/><circle cx="9.5" cy="9" r="1.5"/><path d="m5 18 5-5 3 3 2-2 4 4"/>',
		file: '<path d="M7 3h7l5 5v11a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2z"/><path d="M14 3v5h5"/>',
		userplus: '<circle cx="9" cy="8.5" r="3.2"/><path d="M3 19c0-3.2 2.7-5 6-5s4 .8 5 2.2M18 8v6M15 11h6"/>',
		layers: '<path d="m12 3 9 5-9 5-9-5z"/><path d="m3 12.5 9 5 9-5"/><path d="m3 16.5 9 5 9-5"/>'
	};
	const MONTHS = ['янв', 'фев', 'мар', 'апр', 'мая', 'июн', 'июл', 'авг', 'сен', 'окт', 'ноя', 'дек'];
	const STATUS_CLASS = {
		'На рассмотрении': 'chip-warn',
		'В общем списке': 'chip-neutral',
		'Назначен': 'chip-info',
		'В работе': 'chip-info',
		'На проверке': 'chip-ok',
		'На доработке': 'chip-danger',
		'Приостановлен': 'chip-warn',
		'Завершён': 'chip-ok'
	};
	const PRIORITY_CLASS = { 'Высокий': 'pri-high', 'Средний': 'pri-mid', 'Низкий': 'pri-low' };

	const $ = (selector, root = document) => root.querySelector(selector);
	const $$ = (selector, root = document) => Array.from(root.querySelectorAll(selector));
	const icon = (name, cls = '') => `<svg class="i ${cls}" viewBox="0 0 24 24" aria-hidden="true">${ICONS[name]}</svg>`;
	const esc = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
	const plural = (n, forms) => {
		const a = Math.abs(n) % 100;
		const b = a % 10;
		if (a > 10 && a < 20) return forms[2];
		if (b > 1 && b < 5) return forms[1];
		if (b === 1) return forms[0];
		return forms[2];
	};
	const pad = n => String(n).padStart(2, '0');
	const initials = name => String(name).split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase();
	const fmtDue = iso => {
		const d = new Date(iso);
		return `${pad(d.getDate())} ${MONTHS[d.getMonth()]}, ${pad(d.getHours())}:${pad(d.getMinutes())}`;
	};
	const fmtFull = iso => {
		const d = new Date(iso);
		return `${pad(d.getDate())}.${pad(d.getMonth() + 1)}.${d.getFullYear()} в ${pad(d.getHours())}:${pad(d.getMinutes())}`;
	};
	const fmtShort = iso => {
		const d = new Date(iso);
		return `${pad(d.getDate())} ${MONTHS[d.getMonth()]}, ${pad(d.getHours())}:${pad(d.getMinutes())}`;
	};
	const fmtAgo = iso => {
		const diff = Math.floor((Date.now() - new Date(iso).getTime()) / 60000);
		if (diff < 1) return 'Только что';
		if (diff < 60) return `${diff} мин назад`;
		const d = new Date(iso);
		const today = new Date();
		const yesterday = new Date();
		yesterday.setDate(today.getDate() - 1);
		if (d.toDateString() === today.toDateString()) return `Сегодня, ${pad(d.getHours())}:${pad(d.getMinutes())}`;
		if (d.toDateString() === yesterday.toDateString()) return `Вчера, ${pad(d.getHours())}:${pad(d.getMinutes())}`;
		return fmtShort(iso);
	};
	const fmtSize = bytes => bytes >= 1048576 ? `${(bytes / 1048576).toFixed(1).replace('.', ',')} МБ` : `${Math.max(1, Math.round(bytes / 1024))} КБ`;
	const statusChip = status => `<span class="chip ${STATUS_CLASS[status] || 'chip-neutral'}">${esc(status)}</span>`;
	const priorityClass = priority => PRIORITY_CLASS[priority] || '';

	let toastTimer = null;
	const toast = message => {
		let el = $('#toast');
		if (!el) {
			el = document.createElement('div');
			el.id = 'toast';
			el.className = 'toast';
			el.setAttribute('role', 'status');
			document.body.appendChild(el);
		}
		el.textContent = message;
		el.classList.add('show');
		clearTimeout(toastTimer);
		toastTimer = setTimeout(() => el.classList.remove('show'), 3200);
	};
	const flash = message => sessionStorage.setItem(FLASH_KEY, message);
	const showFlash = () => {
		const message = sessionStorage.getItem(FLASH_KEY);
		if (message) {
			sessionStorage.removeItem(FLASH_KEY);
			toast(message);
		}
	};

	const openModal = id => {
		const modal = document.getElementById(id);
		if (!modal) return;
		modal.classList.add('open');
		const first = $('input:not([readonly]):not([type=hidden]):not([type=file])', modal);
		if (first) first.focus();
	};
	const closeModals = () => $$('.overlay.open').forEach(m => m.classList.remove('open'));

	const setFieldError = (field, message) => {
		if (!field) return;
		field.classList.toggle('invalid', Boolean(message));
		const el = $('.field-error', field);
		if (el && message) el.textContent = message;
	};
	const clearFieldErrors = root => $$('.field.invalid', root).forEach(f => f.classList.remove('invalid'));
	const applyErrors = (root, errors) => {
		clearFieldErrors(root);
		Object.keys(errors).forEach(key => setFieldError($(`[data-field="${key}"]`, root), errors[key]));
	};

	const attachments = ({ button, input, list, errorBox, initial = [] }) => {
		let files = initial.map(f => ({ ...f }));
		const render = () => {
			list.innerHTML = files.map((f, i) => `<div class="file"><span class="ico">${icon(String(f.type).startsWith('image/') ? 'img' : 'file')}</span><div><b>${esc(f.name)}</b><small>${fmtSize(f.size)} · Файл загружен</small></div><button class="x" type="button" data-remove="${i}" aria-label="Убрать файл">${icon('x')}</button></div>`).join('');
		};
		const showErrors = problems => {
			if (!errorBox) return;
			errorBox.classList.toggle('hidden', problems.length === 0);
			errorBox.innerHTML = problems.map(p => `<div class="alert alert-error file-error">${icon('alert')}<div><b>${esc(p.name)} — файл не загружен</b>${esc(p.message)}</div></div>`).join('');
			errorBox.className = problems.length ? 'file-errors' : 'file-errors hidden';
		};
		button.addEventListener('click', () => input.click());
		input.addEventListener('change', () => {
			const problems = [];
			Array.from(input.files).forEach(file => {
				const message = Store.validateFile(file);
				if (message) problems.push({ name: file.name, message });
				else files.push({ name: file.name, size: file.size, type: file.type });
			});
			input.value = '';
			showErrors(problems);
			render();
		});
		list.addEventListener('click', e => {
			const remove = e.target.closest('[data-remove]');
			if (!remove) return;
			files.splice(Number(remove.dataset.remove), 1);
			render();
		});
		render();
		return { get: () => files.map(f => ({ ...f })), clearErrors: () => showErrors([]) };
	};

	document.addEventListener('click', e => {
		const opener = e.target.closest('[data-open]');
		if (opener) {
			e.preventDefault();
			openModal(opener.dataset.open);
			return;
		}
		if (e.target.closest('[data-close]')) {
			e.preventDefault();
			closeModals();
			return;
		}
		if (e.target.classList && e.target.classList.contains('overlay')) closeModals();
	});
	document.addEventListener('keydown', e => {
		if (e.key === 'Escape') closeModals();
	});

	return { $, $$, icon, esc, plural, initials, fmtDue, fmtFull, fmtShort, fmtAgo, fmtSize, statusChip, priorityClass, toast, flash, showFlash, openModal, closeModals, setFieldError, clearFieldErrors, applyErrors, attachments };
})();

const Shell = (() => {
	const { $, icon, esc } = UI;

	const navItems = role => {
		const projects = { key: 'projects', href: 'projects.html', label: 'Проекты', icon: 'projects' };
		if (role === 'manager') {
			return [projects,
				{ key: 'tickets', href: 'tickets.html', label: 'Тикеты', icon: 'tickets' },
				{ key: 'members', href: 'members.html', label: 'Участники', icon: 'users' },
				{ key: 'settings', href: 'project-settings.html', label: 'Настройки', icon: 'settings' }];
		}
		if (role === 'tester') return [projects, { key: 'tickets', href: 'my-tickets.html', label: 'Тикеты', icon: 'tickets' }];
		return [projects];
	};

	const workBlock = (role, projectId) => {
		if (role === 'manager') {
			const count = Store.tickets(projectId).filter(t => t.status === 'На рассмотрении').length;
			return `<div class="side-section">Моя работа</div><a class="side-link" href="tickets.html">Входящие <b>${count}</b></a>`;
		}
		if (role === 'tester') {
			const me = Store.currentUser().id;
			const count = Store.tickets(projectId).filter(t => t.authorId === me && t.status === 'На проверке').length;
			return `<div class="side-section">Моя работа</div><a class="side-link" href="my-tickets.html?tab=review">На проверке <b>${count}</b></a>`;
		}
		return '';
	};

	const renderNotifications = () => {
		const list = Store.notifications();
		const unread = list.filter(n => !n.read).length;
		const items = list.length
			? list.map(n => `<a class="dd-item${n.read ? ' read' : ''}" href="#" data-notification="${n.id}" data-link="${esc(n.link || '')}" data-project="${esc(n.projectId || '')}"><b>${esc(n.text)}</b><small>${UI.fmtAgo(n.at)}</small></a>`).join('')
			: '<div class="dd-empty">Новых уведомлений нет</div>';
		return { unread, html: `<div class="dd-head">Уведомления <span class="count">${unread}</span></div>${items}<button class="dd-foot" id="read-all" type="button">Отметить все прочитанными</button>` };
	};

	const init = ({ roles = null, active = '', project = true, crumbs = [] } = {}) => {
		const params = new URLSearchParams(location.search);
		const user = Store.currentUser();
		if (!user) {
			location.replace('login.html');
			return null;
		}
		const forced = params.get('project');
		if (forced && Store.membership(forced, user.id)) Store.setProjectId(forced);
		if (!project) Store.setProjectId(null);
		const proj = Store.getProjectId() ? Store.project(Store.getProjectId()) : null;
		const member = proj ? Store.membership(proj.id) : null;
		const role = member && member.status === 'active' ? member.role : null;
		if (project && (!proj || !role || (roles && !roles.includes(role)))) {
			location.replace('projects.html');
			return null;
		}

		const nav = navItems(project ? role : null).map(i => `<a href="${i.href}"${i.key === active ? ' class="active"' : ''}>${icon(i.icon)}${i.label}</a>`).join('');
		const sidebar = document.createElement('aside');
		sidebar.className = 'sidebar';
		sidebar.innerHTML = `<a class="brand" href="projects.html"><span class="logo">${icon('layers')}</span>Контур</a>
			<nav class="nav">${nav}</nav>
			${project ? workBlock(role, proj.id) : ''}
			<div class="side-user"><span class="avatar">${UI.initials(user.name)}</span><div><b>${esc(user.name)}</b>${project ? `<small>${Store.ROLES[role]}</small>` : ''}</div></div>`;

		const crumbList = typeof crumbs === 'function' ? crumbs(proj) : crumbs;
		const crumbHtml = crumbList.map(c => typeof c === 'string' ? esc(c) : `<a href="${c[1]}">${esc(c[0])}</a>`).join(' / ');
		const n = renderNotifications();
		const main = $('main.content');
		const wrap = document.createElement('div');
		wrap.className = 'main';
		wrap.innerHTML = `<header class="topbar"><div class="crumbs">${crumbHtml}</div>
			<div class="top-actions"><span class="env">Рабочая среда</span>
			<button class="icon-btn" id="bell" aria-label="Уведомления">${icon('bell')}${n.unread ? '<span class="dot"></span>' : ''}</button>
			<span class="sep"></span><a class="logout" href="login.html" id="logout">Выйти</a></div></header>
			<div class="dropdown" id="notifications" role="dialog" aria-label="Уведомления">${n.html}</div>`;
		main.before(wrap);
		wrap.appendChild(main);
		document.body.prepend(sidebar);

		const dropdown = $('#notifications');
		$('#logout').addEventListener('click', () => Store.setCurrentUser(null));
		$('#bell').addEventListener('click', () => dropdown.classList.toggle('open'));
		dropdown.addEventListener('click', e => {
			if (e.target.closest('#read-all')) {
				Store.markAllRead();
				const fresh = renderNotifications();
				dropdown.innerHTML = fresh.html;
				const dot = $('#bell .dot');
				if (dot) dot.remove();
				return;
			}
			const item = e.target.closest('[data-notification]');
			if (!item) return;
			e.preventDefault();
			Store.markRead(item.dataset.notification);
			const link = item.dataset.link;
			if (link && item.dataset.project) {
				const target = `${link}${link.includes('?') ? '&' : '?'}project=${item.dataset.project}`;
				location.href = target;
			} else {
				item.classList.add('read');
			}
		});
		document.addEventListener('click', e => {
			if (dropdown.classList.contains('open') && !e.target.closest('#notifications') && !e.target.closest('#bell')) dropdown.classList.remove('open');
		});
		document.addEventListener('keydown', e => {
			if (e.key === 'Escape') dropdown.classList.remove('open');
		});
		UI.showFlash();
		return { user, project: proj, member, role, params };
	};

	return { init };
})();
