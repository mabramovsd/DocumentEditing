import { setupMenuNavigation } from './menu.js';

//Authentification
const apiClient = axios.create({
    baseURL: '/'
});

apiClient.interceptors.request.use(function (config) {
    const token = localStorage.getItem('authToken');
    if (token) {
        config.headers['Authorization'] = `Bearer ${token}`;
        config.headers['Content-Type'] = 'application/json';
    }
    return config;
});

//Loading components
async function loadComponent(componentPath, containerId) {
    try {
        const url = `components/${componentPath}.html`;

        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`Failed to load component from ${url}`);
        }
        const htmlContent = await response.text();
        const container = document.getElementById(containerId);
        container.innerHTML = htmlContent;
        
        requestAnimationFrame(() => {
            container.style.opacity = '1';
        });

    } catch (error) {
        console.error(error);
        document.getElementById(containerId).innerHTML = `<p>Ошибка загрузки компонента: ${error.message}</p>`;
    }
}

async function loginToApi(username) {
    //empty password because of model contract
    const messageToSend = JSON.stringify({ email: username, password: '' });

    try {
        const response = await fetch('/Auth/Login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: messageToSend
        });

        if (!response.ok) {
            throw new Error(`Server error: ${response.status}`);
        }

        const data = await response.json();
        return data.token;

    } catch (error) {
        console.error("Failed to log in:", error);
        return null;
    }
}

async function initializeUser() {
    let name = sessionStorage.getItem('user');

    if (!name || name.trim() === '') {
        name = prompt('Please enter your name');

        if (name !== null && name.trim() !== '') {
            name = name.trim();
            sessionStorage.setItem('user', name);

            const token = await loginToApi(name);

            if (token) {
                localStorage.setItem('authToken', token);
                console.log('Successfully logged in. Token received.');
            } else {
                console.warn('Logged in locally, but failed to get API token.');
            }

        } else {
            alert('We will mark you as guest');
            sessionStorage.setItem('user', 'guest');
            console.log('User did not provide a name.');
        }
    } else {
        console.log(`Welcome back, ${name}.`);
    }
}

function setPageTitle(title) {
    document.title = title;
}

// --- Основная логика приложения ---
document.addEventListener('DOMContentLoaded', () => {
    const appContainer = document.getElementById('app');

    // Start page
    loadComponent('Home/Index', 'app').then(() => {
        setPageTitle("Main Page - DocumentEditing");
        //Some animation to show prompt after rendering
        appContainer.addEventListener('transitionend', function handler() {
            appContainer.removeEventListener('transitionend', handler);
            initializeUser();
        });
    });

    setupMenuNavigation(apiClient); 

    // --- Обработчик клика по файлу в списке ---
    // Используем делегирование, чтобы ловить клики по ссылкам внутри таблицы
    document.body.addEventListener('click', async (event) => {
        if (event.target.classList.contains('doc-link')) {
            console.log(event.target);
            event.preventDefault();
            const filename = event.target.dataset.filename;

            // 2. Загружаем компонент редактора
            await loadComponent('Audit/Details', 'app');

            const container = document.getElementById('app');
            const header = container.querySelector('#AuditDetailsHeader');
            const content = container.querySelector('#AuditMainContent');

            // 3. После того как редактор появился, загружаем данные с сервера
            try {
                const response = await apiClient.get(`/Audit/Details/${filename}`);
                setPageTitle("Audit of document - DocumentEditing");
                header.textContent = 'Changes of ' + response.data.fileName;
                content.textContent = response.data.content || '';
            } catch (error) {
                alert(error.response?.data?.error || "Произошла ошибка");
            }
        }
    });
});