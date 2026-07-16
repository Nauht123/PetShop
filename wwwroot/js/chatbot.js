document.addEventListener('DOMContentLoaded', function () {
    const bubble = document.getElementById('chatbotBubble');
    const win = document.getElementById('chatbotWindow');
    const closeBtn = document.getElementById('chatbotClose');
    const messagesEl = document.getElementById('chatbotMessages');
    const input = document.getElementById('chatbotInput');
    const sendBtn = document.getElementById('chatbotSend');

    const STORAGE_KEY_HISTORY = 'petshop_chat_history';
    const STORAGE_KEY_OPEN = 'petshop_chat_open';
    const STORAGE_KEY_POS = 'petshop_chat_bubble_pos';

    let history = [];

    // ══════════════ KHÔI PHỤC LỊCH SỬ CHAT ══════════════
    function loadHistory() {
        try {
            const saved = sessionStorage.getItem(STORAGE_KEY_HISTORY);
            if (saved) {
                history = JSON.parse(saved);
                // Xóa tin chào mặc định, render lại toàn bộ lịch sử đã lưu
                messagesEl.innerHTML = '';
                history.forEach(h => {
                    appendMessage(h.text, h.role === 'user' ? 'user' : 'bot', false);
                });
            }
        } catch (err) {
            history = [];
        }
    }

    function saveHistory() {
        sessionStorage.setItem(STORAGE_KEY_HISTORY, JSON.stringify(history));
    }

    // ══════════════ GHI NHỚ TRẠNG THÁI MỞ/ĐÓNG KHUNG CHAT ══════════════
    function restoreOpenState() {
        if (sessionStorage.getItem(STORAGE_KEY_OPEN) === 'true') {
            win.classList.remove('d-none');
        }
    }

    function saveOpenState(isOpen) {
        sessionStorage.setItem(STORAGE_KEY_OPEN, isOpen ? 'true' : 'false');
    }

    // ══════════════ GHI NHỚ VỊ TRÍ BONG BÓNG ══════════════
    function restoreBubblePosition() {
        const saved = sessionStorage.getItem(STORAGE_KEY_POS);
        if (saved) {
            try {
                const pos = JSON.parse(saved);
                bubble.style.left = pos.left;
                bubble.style.top = pos.top;
                bubble.style.right = 'auto';
                bubble.style.bottom = 'auto';
            } catch (err) { /* bỏ qua nếu lỗi, giữ vị trí mặc định */ }
        }
    }

    function saveBubblePosition() {
        sessionStorage.setItem(STORAGE_KEY_POS, JSON.stringify({
            left: bubble.style.left,
            top: bubble.style.top
        }));
    }

    // ══════════════ KÉO THẢ BONG BÓNG CHAT ══════════════
    let isDragging = false;
    let hasMoved = false;
    let startX, startY, initialLeft, initialTop;

    function getPosition(e) {
        return e.touches ? e.touches[0] : e;
    }

    function dragStart(e) {
        isDragging = true;
        hasMoved = false;

        const pos = getPosition(e);
        const rect = bubble.getBoundingClientRect();

        startX = pos.clientX;
        startY = pos.clientY;
        initialLeft = rect.left;
        initialTop = rect.top;

        bubble.style.left = initialLeft + 'px';
        bubble.style.top = initialTop + 'px';
        bubble.style.right = 'auto';
        bubble.style.bottom = 'auto';

        document.addEventListener('mousemove', dragMove);
        document.addEventListener('touchmove', dragMove, { passive: false });
        document.addEventListener('mouseup', dragEnd);
        document.addEventListener('touchend', dragEnd);
    }

    function dragMove(e) {
        if (!isDragging) return;

        const pos = getPosition(e);
        const dx = pos.clientX - startX;
        const dy = pos.clientY - startY;

        if (Math.abs(dx) > 5 || Math.abs(dy) > 5) {
            hasMoved = true;
        }

        if (hasMoved) {
            e.preventDefault();

            let newLeft = initialLeft + dx;
            let newTop = initialTop + dy;

            const maxLeft = window.innerWidth - bubble.offsetWidth;
            const maxTop = window.innerHeight - bubble.offsetHeight;
            newLeft = Math.max(0, Math.min(newLeft, maxLeft));
            newTop = Math.max(0, Math.min(newTop, maxTop));

            bubble.style.left = newLeft + 'px';
            bubble.style.top = newTop + 'px';
        }
    }

    function dragEnd() {
        isDragging = false;
        document.removeEventListener('mousemove', dragMove);
        document.removeEventListener('touchmove', dragMove);
        document.removeEventListener('mouseup', dragEnd);
        document.removeEventListener('touchend', dragEnd);

        if (hasMoved) {
            bubble.dataset.justDragged = "true";
            saveBubblePosition(); // lưu vị trí mới sau khi kéo xong
            setTimeout(() => { bubble.dataset.justDragged = "false"; }, 50);
        }
    }

    bubble.addEventListener('mousedown', dragStart);
    bubble.addEventListener('touchstart', dragStart, { passive: false });

    // ══════════════ MỞ / ĐÓNG KHUNG CHAT ══════════════
    bubble.addEventListener('click', () => {
        if (bubble.dataset.justDragged === "true") return;
        win.classList.toggle('d-none');
        saveOpenState(!win.classList.contains('d-none'));
    });
    closeBtn.addEventListener('click', () => {
        win.classList.add('d-none');
        saveOpenState(false);
    });

    // ══════════════ GỬI TIN NHẮN ══════════════
    function appendMessage(text, role, scroll = true) {
        const div = document.createElement('div');
        div.className = 'chatbot-msg ' + role;
        div.textContent = text;
        messagesEl.appendChild(div);
        if (scroll) messagesEl.scrollTop = messagesEl.scrollHeight;
        return div;
    }

    async function sendMessage() {
        const text = input.value.trim();
        if (!text) return;

        appendMessage(text, 'user');
        history.push({ role: 'user', text: text });
        saveHistory();
        input.value = '';

        const typingEl = appendMessage('Đang trả lời...', 'bot typing');

        try {
            const res = await fetch('/Chatbot/Ask', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: text, history: history })
            });
            const data = await res.json();

            typingEl.remove();
            appendMessage(data.reply, 'bot');
            history.push({ role: 'bot', text: data.reply });
            saveHistory();
        } catch (err) {
            typingEl.remove();
            appendMessage('Xin lỗi, có lỗi kết nối. Vui lòng thử lại.', 'bot');
        }
    }

    sendBtn.addEventListener('click', sendMessage);
    input.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') sendMessage();
    });

    // ══════════════ KHỞI TẠO KHI TẢI TRANG ══════════════
    restoreBubblePosition();
    loadHistory();
    restoreOpenState();
    messagesEl.scrollTop = messagesEl.scrollHeight;


    function getAntiForgeryToken() {
        return document.querySelector(
            '#__antiForgeryForm input[name="__RequestVerificationToken"]'
        ).value;
    }

    fetch('/Chatbot/Ask', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({ message, history })
    });
});