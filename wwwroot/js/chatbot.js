document.addEventListener('DOMContentLoaded', function () {
    const bubble = document.getElementById('chatbotBubble');
    const win = document.getElementById('chatbotWindow');
    const closeBtn = document.getElementById('chatbotClose');
    const messagesEl = document.getElementById('chatbotMessages');
    const input = document.getElementById('chatbotInput');
    const sendBtn = document.getElementById('chatbotSend');

    let history = [];

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

        // Chuyển từ bottom/right sang left/top để kéo tự do
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

        // Coi là "kéo" nếu di chuyển hơn 5px, tránh nhầm với click
        if (Math.abs(dx) > 5 || Math.abs(dy) > 5) {
            hasMoved = true;
        }

        if (hasMoved) {
            e.preventDefault();

            let newLeft = initialLeft + dx;
            let newTop = initialTop + dy;

            // Giới hạn không cho kéo ra ngoài màn hình
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

        // Nếu vừa kéo (hasMoved), chặn sự kiện click ngay sau đó để không mở khung chat
        if (hasMoved) {
            bubble.dataset.justDragged = "true";
            setTimeout(() => { bubble.dataset.justDragged = "false"; }, 50);
        }
    }

    bubble.addEventListener('mousedown', dragStart);
    bubble.addEventListener('touchstart', dragStart, { passive: false });

    // ══════════════ MỞ / ĐÓNG KHUNG CHAT ══════════════
    bubble.addEventListener('click', () => {
        if (bubble.dataset.justDragged === "true") return; // vừa kéo thì không mở
        win.classList.toggle('d-none');
    });
    closeBtn.addEventListener('click', () => {
        win.classList.add('d-none');
    });

    // ══════════════ GỬI TIN NHẮN ══════════════
    function appendMessage(text, role) {
        const div = document.createElement('div');
        div.className = 'chatbot-msg ' + role;
        div.textContent = text;
        messagesEl.appendChild(div);
        messagesEl.scrollTop = messagesEl.scrollHeight;
        return div;
    }

    async function sendMessage() {
        const text = input.value.trim();
        if (!text) return;

        appendMessage(text, 'user');
        history.push({ role: 'user', text: text });
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
        } catch (err) {
            typingEl.remove();
            appendMessage('Xin lỗi, có lỗi kết nối. Vui lòng thử lại.', 'bot');
        }
    }

    sendBtn.addEventListener('click', sendMessage);
    input.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') sendMessage();
    });
});