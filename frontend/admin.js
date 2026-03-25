const API_URL = "http://localhost:5000";

const token = sessionStorage.getItem("token");
const role = sessionStorage.getItem("role");

if (!token || role !== "admin") {
    window.location.href = "index.html";
}

// Admin adını göster
document.getElementById("admin-username").textContent = sessionStorage.getItem("username");

// Sayfa açılınca yükle
loadUsers();
loadDocuments();

function switchTab(tab) {
    document.getElementById("panel-users").style.display = tab === "users" ? "block" : "none";
    document.getElementById("panel-docs").style.display = tab === "docs" ? "block" : "none";
    document.getElementById("tab-users").classList.toggle("active", tab === "users");
    document.getElementById("tab-docs").classList.toggle("active", tab === "docs");
}

async function loadUsers() {
    const list = document.getElementById("user-list");
    try {
        const response = await fetch(`${API_URL}/api/auth/users`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (response.ok) {
            const users = await response.json();
            list.innerHTML = users.length
                ? users.map(u => `
                    <div class="user-card">
                        <div class="user-info">
                            <i class="bi bi-person-circle"></i>
                            <span class="user-name">${u.username}</span>
                            <span class="user-role ${u.role}">${u.role}</span>
                        </div>
                        ${u.username !== "admin" ? `
                        <button class="delete-btn" onclick="deleteUser('${u.username}')">
                            <i class="bi bi-trash"></i> Sil
                        </button>` : ""}
                    </div>
                `).join("")
                : "<p class='no-result'>Kullanıcı bulunamadı.</p>";
        }
    } catch (error) {
        list.textContent = "Sunucuya bağlanılamadı.";
    }
}

async function addUser() {
    const username = document.getElementById("new-username").value;
    const password = document.getElementById("new-password").value;
    const role = document.getElementById("new-role").value;
    const message = document.getElementById("add-message");

    if (!username || !password) {
        message.style.color = "red";
        message.textContent = "Kullanıcı adı ve şifre boş olamaz.";
        return;
    }

    try {
        const response = await fetch(`${API_URL}/api/auth/register`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ username, password, role })
        });

        if (response.ok) {
            message.style.color = "green";
            message.textContent = "Kullanıcı eklendi!";
            document.getElementById("new-username").value = "";
            document.getElementById("new-password").value = "";
            loadUsers();
        } else {
            const data = await response.json();
            message.style.color = "red";
            message.textContent = data.message || "Hata oluştu.";
        }
    } catch (error) {
        message.style.color = "red";
        message.textContent = "Sunucuya bağlanılamadı.";
    }
}

async function deleteUser(username) {
    if (!confirm(`"${username}" kullanıcısını silmek istediğinize emin misiniz?`)) return;

    try {
        const response = await fetch(`${API_URL}/api/auth/users/${username}`, {
            method: "DELETE",
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (response.ok) loadUsers();
    } catch (error) {
        alert("Sunucuya bağlanılamadı.");
    }
}

async function loadDocuments() {
    const list = document.getElementById("doc-list");
    try {
        const response = await fetch(`${API_URL}/api/documents`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (response.ok) {
            const docs = await response.json();
            list.innerHTML = docs.length
                ? docs.map(d => `
                    <div class="user-card">
                        <div class="user-info">
                            <i class="bi bi-file-earmark-text" style="font-size:24px;"></i>
                            <div>
                                <span class="user-name">${d.title}</span>
                                <span class="user-role user">${d.category}</span>
                            </div>
                        </div>
                        <button class="delete-btn" onclick="deleteDocument('${d.id}')">
                            <i class="bi bi-trash"></i> Sil
                        </button>
                    </div>
                `).join("")
                : "<p class='no-result'>Doküman bulunamadı.</p>";
        }
    } catch (error) {
        list.textContent = "Sunucuya bağlanılamadı.";
    }
}

async function addDocument() {
    const title = document.getElementById("doc-title").value;
    const content = document.getElementById("doc-content").value;
    const category = document.getElementById("doc-category").value;
    const message = document.getElementById("doc-message");
    const username = sessionStorage.getItem("username");

    if (!title || !content || !category) {
        message.style.color = "red";
        message.textContent = "Tüm alanları doldurun.";
        return;
    }

    try {
        const response = await fetch(`${API_URL}/api/documents`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ title, content, category, uploadedBy: username })
        });

        if (response.ok || response.status === 204) {
            message.style.color = "green";
            message.textContent = "Doküman eklendi!";
            document.getElementById("doc-title").value = "";
            document.getElementById("doc-content").value = "";
            document.getElementById("doc-category").value = "";
            loadDocuments();
        } else {
            message.style.color = "red";
            message.textContent = "Hata oluştu.";
        }
    } catch (error) {
        message.style.color = "red";
        message.textContent = "Sunucuya bağlanılamadı.";
    }
}

async function deleteDocument(id) {
    if (!confirm("Bu dokümanı silmek istediğinize emin misiniz?")) return;

    try {
        const response = await fetch(`${API_URL}/api/documents/${id}`, {
            method: "DELETE",
            headers: { "Authorization": `Bearer ${token}` }
        });
        if (response.ok || response.status === 204) loadDocuments();
    } catch (error) {
        alert("Sunucuya bağlanılamadı.");
    }
}

function adminLogout() {
    sessionStorage.clear();
    window.location.href = "index.html";
}
