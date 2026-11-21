const { $, $$ } = UI;

if (Store.currentUser()) location.replace('projects.html');

const forms = { login: $('#login-form'), register: $('#register-form') };
const copy = {
	login: { title: 'Вход в систему', sub: 'Введите почту и пароль, чтобы продолжить работу.' },
	register: { title: 'Регистрация', sub: 'Создайте аккаунт, чтобы работать с проектами и тикетами.' }
};
let mode = 'login';

const setMode = next => {
	mode = next;
	Object.keys(forms).forEach(key => forms[key].classList.toggle('hidden', key !== next));
	$$('[data-mode]').forEach(a => a.classList.toggle('active', a.dataset.mode === next));
	$('#auth-title').textContent = copy[next].title;
	$('#auth-sub').textContent = copy[next].sub;
	$('#auth-error').classList.add('hidden');
	$('#demo-hint').classList.toggle('hidden', next !== 'login');
	UI.clearFieldErrors(document);
	const first = $('input', forms[next]);
	if (first) first.focus();
};

const showError = message => {
	$('#auth-error-text').textContent = message;
	$('#auth-error').classList.remove('hidden');
};

$$('[data-mode]').forEach(a => a.addEventListener('click', e => {
	e.preventDefault();
	setMode(a.dataset.mode);
}));

$$('.pw-toggle').forEach(button => button.addEventListener('click', () => {
	const input = button.parentNode.querySelector('input');
	const visible = input.type === 'text';
	input.type = visible ? 'password' : 'text';
	button.setAttribute('aria-label', visible ? 'Показать пароль' : 'Скрыть пароль');
}));

document.addEventListener('input', e => {
	const field = e.target.closest('.field');
	if (field) UI.setFieldError(field, '');
	$('#auth-error').classList.add('hidden');
});

const withBusy = async (form, action) => {
	const button = $('button[type=submit]', form);
	button.disabled = true;
	try {
		await action();
	} catch (error) {
		showError('Не удалось выполнить запрос. Откройте сайт через Live Server или https.');
	} finally {
		button.disabled = false;
	}
};

forms.login.addEventListener('submit', e => {
	e.preventDefault();
	UI.clearFieldErrors(forms.login);
	const email = forms.login.email.value.trim();
	const password = forms.login.password.value;
	if (!email || !password) {
		if (!email) UI.setFieldError($('[data-field="email"]', forms.login), 'Введите почту');
		if (!password) UI.setFieldError($('[data-field="password"]', forms.login), 'Введите пароль');
		return;
	}
	withBusy(forms.login, async () => {
		const result = await Auth.login(email, password);
		if (!result.ok) {
			showError(result.error);
			return;
		}
		location.href = 'projects.html';
	});
});

forms.register.addEventListener('submit', e => {
	e.preventDefault();
	withBusy(forms.register, async () => {
		const result = await Auth.register({
			name: forms.register.name.value,
			email: forms.register.email.value,
			password: forms.register.password.value,
			confirm: forms.register.confirm.value
		});
		if (!result.ok) {
			UI.applyErrors(forms.register, result.errors);
			return;
		}
		UI.flash(`Добро пожаловать, ${result.user.name.split(' ')[0]}! Аккаунт создан.`);
		location.href = 'projects.html';
	});
});

setMode(new URLSearchParams(location.search).get('mode') === 'register' ? 'register' : 'login');
