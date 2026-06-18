/**
 * Click on top menu handler
 */
export function setupMenuNavigation(apiClient) {
    document.body.addEventListener('click', async (event) => {
        //Where do we click
        const link = event.target.closest('a');

        // Click on Docs
        if (link && link.getAttribute('href')?.startsWith('/Documents')) {
            event.preventDefault();
            await loadAndFillDocumentsComponent('app', apiClient);
            setPageTitle("Documents - DocumentEditing");
        }
        // Click on Audit
        if (link && link.getAttribute('href')?.startsWith('/Audit')) {
            event.preventDefault();
            await loadAndFillAuditComponent('app', apiClient);
            setPageTitle("Audit - DocumentEditing");
        }
        // Click on Home
        if (link && link.getAttribute('href')?.startsWith('/Home')) {
            event.preventDefault();
            loadComponent('Home/Index', 'app');
            setPageTitle("Main Page - DocumentEditing");
        }
    });
}

/**
 * Page title
 */
function setPageTitle(title) {
    document.title = title;
}

/**
 * Page rendering
 */
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
        document.getElementById(containerId).innerHTML = `<p>Error while loading component: ${error.message}</p>`;
    }
}

/**
 * Rendering of Audit page
 */
async function loadAndFillAuditComponent(containerId, apiClient) {
    //HTML-template (table header)
    await loadComponent('Audit/Index', containerId);

    const container = document.getElementById(containerId);
    const tbody = container.querySelector('#AuditTable tbody');

    try {
        const response = await apiClient.get('Api/Audit/Index');

        //Filling Table Body
        tbody.innerHTML = '';
        response.data.documents.forEach(item => {
            const rowHtml = `
                <tr>
                    <td><a href="#" class="audit-link" data-filename="${item.fileName}">${item.fileName}</a></td>
                    <td>${new Date(item.creationTime).toLocaleString()}</td>
                    <td>${new Date(item.lastWriteTime).toLocaleString()}</td>
                    <td>${item.sizeKB} kb</td>
                </tr>
            `;
            tbody.insertAdjacentHTML('beforeend', rowHtml);
        });

    } catch (error) {
        console.error("Failed to load audit data:", error);
        tbody.innerHTML = '<tr><td colspan="4">Failed to load audit data.</td></tr>';
    }
}
/**
 * Rendering of Documents page
 */
async function loadAndFillDocumentsComponent(containerId, apiClient) {
    //HTML-template (table header)
    await loadComponent('Documents/Index', containerId);

    const container = document.getElementById(containerId);
    const tbody = container.querySelector('#DocumentsTable tbody');

    try {
        const response = await apiClient.get('Documents/Index');

        //Filling Table Body
        tbody.innerHTML = '';
        response.data.documents.forEach(item => {
            const rowHtml = `
                <tr>
                    <td><a href="#" class="doc-link" data-filename="${item.fileName}">${item.fileName}</a></td>
                    <td>${new Date(item.creationTime).toLocaleString()}</td>
                    <td>${new Date(item.lastWriteTime).toLocaleString()}</td>
                    <td>${item.sizeKB} kb</td>
                </tr>
            `;
            tbody.insertAdjacentHTML('beforeend', rowHtml);
        });

    } catch (error) {
        console.error("Failed to load documents data:", error);
        tbody.innerHTML = '<tr><td colspan="4">Failed to load documents data.</td></tr>';
    }
}