const ctx = Shell.init({ project: false, active: 'projects', crumbs: ['Рабочее пространство', 'Проекты'] });
const { $, esc } = UI;

const list = $('#projects');
const searchInput = $('#project-search');
const roleLabel = member => member.role ? `Вы — ${Store.ROLES[member.role].toLowerCase()}` : 'Роль не назначена';

const cardHtml = card => {
	const { project, member } = card;
	const gear = member.role === 'manager'
		? `<a class="gear" href="project-settings.html?project=${project.id}" aria-label="Настройки проекта «${esc(project.name)}»">${UI.icon('settings')}</a>`
		: '';
	return `<article class="project">
		<a class="link-card" href="#" data-enter="${project.id}" aria-label="Открыть проект «${esc(project.name)}»"></a>
		${gear}
		<div class="code">${esc(project.code)}</div>
		<h3>${esc(project.name)}</h3>
		<p class="desc-clamp">${esc(project.description)}</p>
		<div class="stats">
			<div><strong>${card.openTickets}</strong><span>${UI.plural(card.openTickets, ['открытый тикет', 'открытых тикета', 'открытых тикетов'])}</span></div>
			<div><strong>${card.membersCount}</strong><span>${UI.plural(card.membersCount, ['участник', 'участника', 'участников'])}</span></div>
		</div>
		<div class="meta">${UI.icon('branch')}${esc(Store.repoLabel(project.repo) || 'Репозиторий не указан')}</div>
		<div class="meta role"><span class="avatar sm">${UI.initials(ctx.user.name)}</span>${roleLabel(member)}</div>
		<div class="upd">Обновлён: ${UI.fmtAgo(project.updatedAt)}</div>
	</article>`;
};

const render = () => {
	const all = Store.projectCards();
	const query = searchInput.value.trim().toLowerCase();
	const cards = all.filter(c => !query || c.project.name.toLowerCase().includes(query) || c.project.code.toLowerCase().includes(query));
	const firstName = ctx.user.name.split(' ')[0];
	$('#welcome-text').textContent = all.length
		? `${firstName}, вам доступно ${all.length} ${UI.plural(all.length, ['проект', 'проекта', 'проектов'])}. Выберите проект, чтобы продолжить работу.`
		: `${firstName}, у вас пока нет проектов. Создайте проект или войдите по коду приглашения.`;
	$('#projects-title').textContent = `Все проекты · ${all.length}`;
	if (!cards.length) {
		list.innerHTML = `<div class="empty" style="grid-column:1/-1"><b>${all.length ? 'Ничего не найдено' : 'Проектов пока нет'}</b>${all.length ? 'Измените запрос поиска.' : 'Создайте новый проект или войдите в существующий по коду приглашения.'}</div>`;
		return;
	}
	list.innerHTML = cards.map(cardHtml).join('');
};

const enter = id => {
	const member = Store.membership(id);
	if (!member || member.status !== 'active' || !member.role) {
		UI.toast('Руководитель ещё не назначил вам роль в этом проекте');
		return;
	}
	Store.setProjectId(id);
	if (member.role === 'manager') location.href = 'tickets.html';
	else if (member.role === 'tester') location.href = 'my-tickets.html';
	else UI.toast('Раздел разработчика пока не реализован');
};

list.addEventListener('click', e => {
	const link = e.target.closest('[data-enter]');
	if (!link) return;
	e.preventDefault();
	enter(link.dataset.enter);
});

$('#search-toggle').addEventListener('click', () => {
	searchInput.classList.toggle('open');
	if (searchInput.classList.contains('open')) searchInput.focus();
	else {
		searchInput.value = '';
		render();
	}
});
searchInput.addEventListener('input', render);

const codeInput = $('#invite-code');
const codeField = $('#invite-field');
const confirmJoin = () => {
	const result = Store.joinByCode(codeInput.value);
	if (!result.ok) {
		UI.setFieldError(codeField, 'Неверный код приглашения');
		return;
	}
	UI.closeModals();
	codeInput.value = '';
	render();
	UI.toast(`Вы вошли в проект «${result.project.name}». Руководитель назначит вам роль.`);
};
$('#join-confirm').addEventListener('click', confirmJoin);
codeInput.addEventListener('input', () => UI.setFieldError(codeField, ''));
codeInput.addEventListener('keydown', e => {
	if (e.key === 'Enter') confirmJoin();
});
$('[data-open="modal-join"]').addEventListener('click', () => {
	codeInput.value = '';
	UI.setFieldError(codeField, '');
});

render();
