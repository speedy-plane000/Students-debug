const Auth = (() => {
	const CREDENTIALS_KEY = 'kontur.credentials.v1';
	const DEMO_PASSWORD = 'kontur123';
	const EMAIL_PATTERN = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
	const LOGIN_ERROR = 'Неверная почта или пароль';

	const loadCredentials = () => JSON.parse(localStorage.getItem(CREDENTIALS_KEY) || '{}');
	const saveCredentials = data => localStorage.setItem(CREDENTIALS_KEY, JSON.stringify(data));
	const normalize = email => String(email || '').trim().toLowerCase();

	const randomSalt = () => Array.from(crypto.getRandomValues(new Uint8Array(16))).map(b => b.toString(16).padStart(2, '0')).join('');
	const hash = async (salt, password) => {
		const bytes = new TextEncoder().encode(`${salt}:${password}`);
		const digest = await crypto.subtle.digest('SHA-256', bytes);
		return Array.from(new Uint8Array(digest)).map(b => b.toString(16).padStart(2, '0')).join('');
	};
	const makeRecord = async (userId, password) => {
		const salt = randomSalt();
		return { userId, salt, hash: await hash(salt, password) };
	};

	const ensureDemoCredentials = async () => {
		const credentials = loadCredentials();
		let changed = false;
		for (const user of Store.db().users) {
			const key = normalize(user.email);
			if (!credentials[key] || credentials[key].userId !== user.id) {
				credentials[key] = await makeRecord(user.id, DEMO_PASSWORD);
				changed = true;
			}
		}
		if (changed) saveCredentials(credentials);
	};

	const login = async (email, password) => {
		await ensureDemoCredentials();
		const record = loadCredentials()[normalize(email)];
		if (!record || !Store.user(record.userId)) return { ok: false, error: LOGIN_ERROR };
		if (await hash(record.salt, String(password || '')) !== record.hash) return { ok: false, error: LOGIN_ERROR };
		Store.setCurrentUser(record.userId);
		return { ok: true };
	};

	const validateRegistration = data => {
		const errors = {};
		const name = String(data.name || '').trim().replace(/\s+/g, ' ');
		const password = String(data.password || '');
		if (name.split(' ').length < 2 || name.length < 3) errors.name = 'Введите имя и фамилию';
		else if (name.length > 80) errors.name = 'Не длиннее 80 символов';
		if (!EMAIL_PATTERN.test(String(data.email || '').trim())) errors.email = 'Введите корректную почту';
		else if (Store.userByEmail(data.email)) errors.email = 'Аккаунт с такой почтой уже существует';
		if (password.length < 8 || !/[A-Za-zА-Яа-я]/.test(password) || !/\d/.test(password)) errors.password = 'Не короче 8 символов, с буквами и цифрами';
		if (!errors.password && password !== data.confirm) errors.confirm = 'Пароли не совпадают';
		else if (errors.password && !data.confirm) errors.confirm = 'Повторите пароль';
		return errors;
	};

	const register = async data => {
		await ensureDemoCredentials();
		const errors = validateRegistration(data);
		if (Object.keys(errors).length) return { ok: false, errors };
		const user = {
			id: `u${Date.now().toString(36)}${Math.random().toString(36).slice(2, 5)}`,
			name: String(data.name).trim().replace(/\s+/g, ' '),
			email: String(data.email).trim()
		};
		Store.db().users.push(user);
		Store.persist();
		const credentials = loadCredentials();
		credentials[normalize(user.email)] = await makeRecord(user.id, data.password);
		saveCredentials(credentials);
		Store.setCurrentUser(user.id);
		return { ok: true, user };
	};

	const logout = () => Store.setCurrentUser(null);

	return { DEMO_PASSWORD, login, register, logout, ensureDemoCredentials };
})();
