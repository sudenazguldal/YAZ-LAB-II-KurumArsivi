const API_URL = "http://localhost:5000";

const token = sessionStorage.getItem("token");
const role = sessionStorage.getItem("role");

// Admin değilse geri gönder
if (!token || role !== "admin") {
    window.location.href = "index.html";
}

// Sayfa açılınca kullanıcıları yükle
loadUsers();

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

        if (response.ok) {
            loadUsers();
        }
    } catch (error) {
        alert("Sunucuya bağlanılamadı.");
    }
}

function adminLogout() {
    sessionStorage.clear();
    window.location.href = "index.html";
}