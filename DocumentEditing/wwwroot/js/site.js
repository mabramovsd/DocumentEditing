import { setupMenuNavigation } from './menu.js';
import { loadAndFillDocumentsComponent } from './menu.js';

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
        document.getElementById(containerId).innerHTML = `<p>Error when loading component: ${error.message}</p>`;
    }
}

async function loginToApi(username) {
    //empty password because of model contract
    const messageToSend = JSON.stringify({ email: username, password: '' });

    try {
        const response = await fetch('Api/Auth/Login', {
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

        document.getElementById("UserNameDiv").innerHTML = "Hello, " + name;
    } else {
        console.log(`Welcome back, ${name}.`);
    }
}

function setPageTitle(title) {
    document.title = title;
}


async function updateTextArea() {
    const textarea = document.getElementById('DocMainContent');
    const content = textarea.value;

    // Some data from session
    const fileName = sessionStorage.getItem('file');
    const userFromStorage = sessionStorage.getItem('user');

    if (!fileName) {
        console.error('File name not found in session storage.');
        return;
    }

    const messageToSend = JSON.stringify({ fileName, content, user: userFromStorage });

    try {
        const response = await fetch('Api/Documents/Save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: messageToSend
        });

        if (response.ok) {
            console.log('Successfully saved!');
        } else {
            console.log('Error while saving');
        }
    } catch (error) {
        console.error("Error while saving: ", error.message);
        document.getElementById("Editors").innerHTML = "Unknown editors";
    }

    const hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("/chat")
        .build();


    hubConnection.on("Receive", function (message) {
        try {
            console.log('received');
            const parsedMessage = JSON.parse(message);
            console.log(parsedMessage);
            let users = parsedMessage?.ActiveEditors ?? [];
            users = users.join(", ");
            document.getElementById("Editors").innerHTML = "Now edit: " + users;
        } catch (error) {
            console.error("Error while parsing message:", error.message);
            document.getElementById("Editors").innerHTML = "Unknown editors";
        }
    });
    hubConnection.start()
        .then(function () {
            hubConnection.invoke("Send", messageToSend)
                .catch(function (err) {
                    return console.error(err.toString());
                });
        })
        .catch(function (err) {
            return console.error(err.toString());
        });
}

function showHideUserButtons(userLoggedId) {
    const loginBtn = document.getElementById('LoginBtn');
    const changeUserBtn = document.getElementById('ChangeUserBtn');
    if (loginBtn) {
        loginBtn.style.display = userLoggedId ? 'none' : 'inline-block';
    }

    if (changeUserBtn) {
        changeUserBtn.style.display = userLoggedId ? 'inline-block' : 'none';
    }
}

// Main logic
document.addEventListener('DOMContentLoaded', () => {
    const appContainer = document.getElementById('app');

    setupMenuNavigation(apiClient);

    // Start page
    loadComponent('Home/Index', 'app').then(() => {
        setPageTitle("Main Page - DocumentEditing");
        //Some animation to show prompt after rendering
        appContainer.addEventListener('transitionend', function handler() {
            appContainer.removeEventListener('transitionend', handler);
            showHideUserButtons(false);
        });
    });

    window.addEventListener('beforeunload', async (event) => {
        const currentFileName = sessionStorage.getItem('file');
        const currentUser = sessionStorage.getItem('user');
        if (!currentFileName || !currentUser) {
            return;
        }

        try {
            await fetch('/Documents/Close', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ fileName: currentFileName, user: currentUser })
            });
            console.log('Successfully sent close signal for:', currentFileName);
        } catch (error) {
            console.error("Error while sending close signal:", error.message);
        }
    });

    // Click on doc handler
    document.body.addEventListener('click', async (event) => {

        if (event.target.id === 'LoginBtn' || event.target.closest('#LoginBtn')) {
            initializeUser();
            showHideUserButtons(true);
        }

        if (event.target.id === 'ChangeUserBtn' || event.target.closest('#ChangeUserBtn')) {
            sessionStorage.removeItem('user');
            initializeUser();
            showHideUserButtons(true);
        }

        if (event.target.id === 'CreateNewBtn' || event.target.closest('#CreateNewBtn')) {
            const fileName = prompt("Please enter file name:");
            const userFromStorage = sessionStorage.getItem('user');

            if (fileName === null || fileName.trim() === '') {
                alert("File Name can't be empty");
                return;
            }

            // Add extension
            const finalFileName = fileName.trim().match(/\.[0-9a-z]+$/i) ? fileName.trim() : fileName.trim() + '.txt';

            fetch('Documents/Create', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ fileName: finalFileName, content: "", user: userFromStorage })
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Error when creating file');
                    }
                })
                .then(data => {
                    loadAndFillDocumentsComponent('app', apiClient);
                })
                .catch(error => {
                    console.error('Error:', error);
                });
        }

        if (event.target.classList.contains('audit-link')) {
            event.preventDefault();
            const filename = event.target.dataset.filename;

            // Loading container
            await loadComponent('Audit/Details', 'app');
            const container = document.getElementById('app');
            const header = container.querySelector('#AuditDetailsHeader');
            const content = container.querySelector('#AuditMainContent');

            // Page content rendering
            try {
                const response = await apiClient.get(`/Api/Audit/Details/${filename}`);
                setPageTitle("Audit of document - DocumentEditing");
                header.textContent = 'Changes of ' + response.data.fileName;
                content.textContent = response.data.content || '';
            } catch (error) {
                alert(error.response?.data?.error || "Error when load document");
            }
        }
        if (event.target.classList.contains('doc-link')) {
            event.preventDefault();
            const filename = event.target.dataset.filename;

            // Loading container
            await loadComponent('Documents/Edit', 'app');
            const container = document.getElementById('app');
            const header = container.querySelector('#DocumentHeader');
            const content = container.querySelector('#DocMainContent');

            // Page content rendering
            try {
                const response = await apiClient.get(`Api/Documents/Edit/${filename}`);
                setPageTitle("Editing of document - DocumentEditing");
                console.log(response);
                header.textContent = 'Editing of ' + response.data.fileName;
                content.textContent = response.data.content || '';
                sessionStorage.setItem('file', response.data.fileName);
                
                document.getElementById('DocMainContent').readOnly = response.data.isReadOnly;

            } catch (error) {
                alert(error.response?.data?.error || "Error when load document");
            }
        }
    });
    appContainer.addEventListener('focusout', function (event) {
        if (event.target.id === 'DocMainContent' && !appContainer.contains(event.relatedTarget)) {
            updateTextArea();
        }
    });
});