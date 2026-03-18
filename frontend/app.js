const API_URL = "http://localhost:5000";


async function login() {
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;
    const message = document.getElementById("login-message");

    if (!username || !password) {
        message.style.color = "red";
        message.textContent = "Kullanıcı adı ve şifre boş olamaz.";
        return;
    }

    try {
        const response = await fetch(`${API_URL}/api/auth/login`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ username, password })
        });

        if (response.ok) {
            const data = await response.json();
            sessionStorage.setItem("token", data.token);
            showMainPage();
        } else {
            message.style.color = "red";
            message.textContent = "Kullanıcı adı veya şifre hatalı.";
        }
    } catch (error) {
        message.style.color = "red";
        message.textContent = "Sunucuya bağlanılamadı.";
    }
}

async function loadDocuments() {
     const results = document.getElementById("search-results");
    const token = sessionStorage.getItem("token");

    try {
        const response = await fetch(`${API_URL}/api/documents`, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (response.ok) {
            const data = await response.json();
            results.innerHTML = data.length
                ? data.map(d => `
                    <div class="document-card" onclick="showDocument('${d.title}', '${d.content}', '${d.category}', '${d.uploadedBy}')">
                        <i class="bi bi-file-earmark-text"></i>
                        <div class="document-info">
                            <p class="document-title">${d.title}</p>
                            <p class="document-date">${new Date(d.createdAt).toLocaleDateString("tr-TR")}</p>
                        </div>
                        <i class="bi bi-chevron-right" style="margin-left:auto;color:#2d6aad;"></i>
                    </div>
                `).join("")
                : "<p class='no-result'>Belge bulunamadı.</p>";
        }
    } catch (error) {
        results.textContent = "Sunucuya bağlanılamadı.";
    }
}

function showMainPage() {
    document.getElementById("login-page").style.display = "none";
    document.getElementById("main-page").style.display = "block";
    loadDocuments();
}

//oturum sonlandırılnca girs sayfasına yönlendirem fonk
function logout() {
    sessionStorage.removeItem("token");//kayıtlı token siliniyor
    document.getElementById("login-page").style.display = "flex";
    document.getElementById("main-page").style.display = "none";
}
//kayıtlı belgeler için arama fonk
async function searchDocuments() {
    const query = document.getElementById("search-input").value;
    const results = document.getElementById("search-results");
    const token = sessionStorage.getItem("token");

    if (!query) {
        results.textContent = "Arama terimi boş olamaz.";
        return;
    }

    try {
        const response = await fetch(`${API_URL}/api/search?q=${query}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (response.ok) {
            const data = await response.json();
           results.innerHTML = data.length
    ? data.map(d => `
        <div class="document-card">
            <i class="bi bi-file-earmark-text"></i>
            <div class="document-info">
                <p class="document-title">${d.title}</p>
                <p class="document-date">${d.date ? new Date(d.date).toLocaleDateString("tr-TR") : "Tarih yok"}</p>
            </div>
        </div>
    `).join("")
    : "<p class='no-result'>Sonuç bulunamadı.</p>";
        } else {
            results.textContent = "Arama başarısız.";
        }
    } catch (error) {
        results.textContent = "Sunucuya bağlanılamadı.";
    }
    results.innerHTML = data.length
    ? data.map(d => `
        <div class="document-card" onclick="showDocument('${d.title}', '${d.content}', '${d.category}', '${d.uploadedBy}')">
            <i class="bi bi-file-earmark-text"></i>
            <div class="document-info">
                <p class="document-title">${d.title}</p>
                <p class="document-date">${d.date ? new Date(d.date).toLocaleDateString("tr-TR") : "Tarih yok"}</p>
            </div>
            <i class="bi bi-chevron-right" style="margin-left:auto;color:#2d6aad;"></i>
        </div>
    `).join("")
    : "<p class='no-result'>Sonuç bulunamadı.</p>";
}

function showDocument(title, content, category, uploadedBy) {
    document.getElementById("modal-title").textContent = title;
    document.getElementById("modal-content").textContent = content;
    document.getElementById("modal-category").textContent = category;
    document.getElementById("modal-uploadedby").textContent = uploadedBy;
    document.getElementById("document-modal").style.display = "flex";
}

function closeModal() {
    document.getElementById("document-modal").style.display = "none";
}
