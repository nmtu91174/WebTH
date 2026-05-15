// Floating Chat Widget Logic
let isLoading = false;
let firstMessageSent = false;

// Toggle chat box
$('#chat-toggle-btn').on('click', function() {
    $('#chat-box').toggle();
    if ($('#chat-box').is(':visible')) {
        $('#widget-message').focus();
    }
});

// Close chat box
$('#chat-close-btn').on('click', function() {
    $('#chat-box').hide();
});

// Auto-expand textarea
$('#widget-message').on('input', function() {
    this.style.height = 'auto';
    this.style.height = Math.min(this.scrollHeight, 80) + 'px';
});

// Clear empty state on first message
function clearEmptyState() {
    if (!firstMessageSent) {
        $('#widget-messages').empty();
        firstMessageSent = true;
    }
}

// Send message
function sendMessage() {
    var userMessage = $('#widget-message').val().trim();
    if (!userMessage || isLoading) return;

    clearEmptyState();

    // Add user message
    const userMessageEl = $('<div class="message-wrapper user"><div class="message user"></div></div>');
    userMessageEl.find('.message').text(userMessage);
    $('#widget-messages').append(userMessageEl);
    $('#widget-message').val('').css('height', 'auto');
    $('#widget-messages').scrollTop($('#widget-messages')[0].scrollHeight);

    // Disable button and show loading
    isLoading = true;
    $('#widget-sendBtn').prop('disabled', true);

    // Show typing indicator
    const typingEl = $('<div class="message-wrapper bot"><div class="typing"><span></span><span></span><span></span></div></div>');
    $('#widget-messages').append(typingEl);
    $('#widget-messages').scrollTop($('#widget-messages')[0].scrollHeight);

    // Request to AI
    $.ajax({
        url: '/ChatBotAI/AskAI',
        type: 'POST',
        data: { message: userMessage },
        success: function (res) {
            typingEl.remove();
            typeBotReply(res.reply);
        },
        error: function (xhr, status, error) {
            typingEl.remove();
            const errorMsg = 'Lỗi kết nối. Vui lòng thử lại.';
            typeBotReply(errorMsg);
        },
        complete: function() {
            isLoading = false;
            $('#widget-sendBtn').prop('disabled', false);
            $('#widget-message').focus();
        }
    });
}

// Send button click
$('#widget-sendBtn').on('click', function() {
    sendMessage();
});

// Send with Enter key
$('#widget-message').on('keydown', function(e) {
    if (e.which === 13) {
        if (e.shiftKey || e.ctrlKey) {
            return; // Allow Shift+Enter for newline
        } else {
            e.preventDefault();
            sendMessage();
        }
    }
});

// Type bot reply with effect
function typeBotReply(text) {
    const container = $('<div class="message-wrapper bot"><div class="message bot"></div></div>');
    const messageEl = container.find('.message');
    $('#widget-messages').append(container);
    $('#widget-messages').scrollTop($('#widget-messages')[0].scrollHeight);

    let i = 0;
    const speed = 15;
    const displayText = [];

    function type() {
        if (i < text.length) {
            const char = text[i];
            if (char === '\n') {
                displayText.push('<br>');
            } else {
                displayText.push(escapeHtml(char));
            }
            messageEl.html(displayText.join(''));
            i++;
            $('#widget-messages').scrollTop($('#widget-messages')[0].scrollHeight);
            setTimeout(type, speed);
        }
    }

    type();
}

// Escape HTML to prevent XSS
function escapeHtml(text) {
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return text.replace(/[&<>"']/g, m => map[m]);
}
