document.addEventListener('DOMContentLoaded', function () {
    const tabAI = document.getElementById('tabAI');
    const tabLive = document.getElementById('tabLive');
    const aiMessages = document.getElementById('chatbotMessages');
    const aiInputArea = document.getElementById('chatbotInputArea');
    const liveMessages = document.getElementById('liveChatMessages');
    const liveInputArea = document.getElementById('liveChatInputArea');
    const liveInput = document.getElementById('liveChatInput');
    const liveSend = document.getElementById('liveChatSend');

    let conversationId = null;
    let connection = null;
    let liveInitialized = false;

    function getAntiForgeryToken() {
        const el = document.querySelector(
            '#__antiForgeryForm input[name="__RequestVerificationToken"]');
        return el ? el.value : '';
    }

    function appendLiveMessage(text, isAdmin, time) {
        const div = document.createElement('div');
        div.className = 'chatbot-msg ' + (isAdmin ? 'bot' : 'user');
        div.textContent = text;
        liveMessages.appendChild(div);
        liveMessages.scrollTop = liveMessages.scrollHeight;
    }

    // ── CHUYỂN TAB ──────────────────────────────────
    tabAI.addEventListener('click', () => {
        tabAI.classList.add('fw-bold', 'btn-light');
        tabAI.classList.remove('btn-outline-light');
        tabLive.classList.remove('fw-bold', 'btn-light');
        tabLive.classList.add('btn-outline-light');

        aiMessages.classList.remove('d-none');
        aiInputArea.classList.remove('d-none');
        liveMessages.classList.add('d-none');
        liveInputArea.classList.add('d-none');
    });

    tabLive.addEventListener('click', async () => {
        tabLive.classList.add('fw-bold', 'btn-light');
        tabLive.classList.remove('btn-outline-light');
        tabAI.classList.remove('fw-bold', 'btn-light');
        tabAI.classList.add('btn-outline-light');

        liveMessages.classList.remove('d-none');
        liveInputArea.classList.remove('d-none');
        aiMessages.classList.add('d-none');
        aiInputArea.classList.add('d-none');

        if (!liveInitialized) {
            liveInitialized = true;
            await initLiveChat();
        }
    });

    // ── KHỞI TẠO CUỘC TRÒ CHUYỆN VỚI ADMIN ──────────
    async function initLiveChat() {
        try {
            const res = await fetch('/LiveChat/Init');
            const data = await res.json();
            conversationId = data.conversationId;

            liveMessages.innerHTML = '';
            if (data.messages.length === 0) {
                appendLiveMessage(
                    'Xin chào! Nhắn tin cho chúng tôi, Admin sẽ phản hồi sớm nhất 👋',
                    true, '');
            } else {
                data.messages.forEach(m => {
                    appendLiveMessage(m.noiDung, m.laAdmin, m.ngayGui);
                });
            }

            // Kết nối SignalR để nhận tin nhắn realtime từ Admin
            connection = new signalR.HubConnectionBuilder()
                .withUrl("/chatHub")
                .withAutomaticReconnect()
                .build();

            connection.on("ReceiveMessage", function (msg) {
                if (msg.id !== conversationId && msg.Id !== conversationId) return;
                appendLiveMessage(
                    msg.noiDung || msg.NoiDung,
                    msg.laAdmin ?? msg.LaAdmin,
                    msg.ngayGui || msg.NgayGui);
            });

            await connection.start();
            await connection.invoke("JoinConversation", conversationId.toString());
        } catch (err) {
            appendLiveMessage('Không thể kết nối hỗ trợ trực tuyến. Vui lòng thử lại sau.', true, '');
        }
    }

    // ── GỬI TIN NHẮN ────────────────────────────────
    async function sendLiveMessage() {
        const text = liveInput.value.trim();
        if (!text || !conversationId) return;

               liveInput.value = '';

        await fetch('/LiveChat/Send', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: `conversationId=${conversationId}&noiDung=${encodeURIComponent(text)}`
        });
    }

    liveSend.addEventListener('click', sendLiveMessage);
    liveInput.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') sendLiveMessage();
    });
});