var clientUser;
var chatapp;
var robot; 

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.start().then(function() {
    console.log("Connected to SignalR hub");
}).catch(function(err) {
    return console.error(err);
});

connection.on("ReceiveMessage", function (chatRoomID, senderID, message) {
    console.log(`chatRoomID: ${chatRoomID},senderID: ${senderID}, message: ${message}`);
    const chatWindow = chatapp.windows.find(c => c?.chatRoomID === chatRoomID);
    const receiveMessage = chatWindow.createMessage(
        chatWindow.members.find(m => m.userID == senderID),
        message,
        chatWindow.nowTimestramp,
    );

    chatWindow.messages.push(receiveMessage);

    chatWindow.WindowElement.messageContainer.append(receiveMessage.element);
    chatWindow.scrollToBottom();
});

connection.on("UserStatusChange", function (userID, status) {
    console.log(userID, status);
    chatapp.FriendWindow.friends.forEach((friend) => {
        if (friend.userID === userID) {
            friend.isOnline = (status == "online");
            console.log(`status-${status}`);
            friend.element.detach().appendTo(`.status-${status}`);
        }
    })
})

// function sendMessage(chatRoomID, message) {
//     connection.invoke("SendMessage", chatRoomID, message).catch(function (err) {
//         return console.error(err);
//     });
// }

$(function () {

    robot = new User({
        "userId": 0,
        "firstName": "AI",
        "lastName": "小幫手",
        "dateOfbirth": "2000-07-28",
        "bio": "?",
        "profilePictureUrl": "/img/robot.png",
        "createdAt": "2000-07-28T00:00:00",
        "updateAt": "2024-07-04T00:00:00",
    })

    $.get("/api/User/Profile", function (resp) {
        clientUser = new User(resp);
    })
        .done(() => {
            chatapp = new ChatApp(clientUser);
            $("body").append(chatapp.element);
        })
        .fail((err) => {
            console.log(err);
        })
        .always(() => {
            
        });
});

class User {
    constructor(userData) {
        this.userID = userData.userId;
        this.firstName = userData.firstName;
        this.lastName = userData.lastName;
        this.birthDate = userData.dateOfbirth;
        this.gender = userData.bio;
        this.avatarUrl = userData.profilePictureUrl;
        this.createdAt = userData.createdAt;
    }
}

class UserFriend extends User {
    static #ElementBuilder = class FriendElementBuilder{
        constructor(user) {
            this.element = $(`
                <div class="friend-item">
                    <i class="status-light"></i>
                    <img class="friend-avatar" height="80%" src="${user.avatarUrl}" alt="friend-avatar">
                    <h6 class="friend-name">${user.firstName + user.lastName}</h6>
                </div>
            `)
        }
    }

    constructor(userFriendsData) {
        super(userFriendsData.profile);
        this.friendElement = new UserFriend.#ElementBuilder(this);
        this.isOnline = userFriendsData.isOnline;
    }

    get element() {
        return this.friendElement.element
    }
}

class ChatRoomMember extends User {
    constructor(chatRoomMemberData) {
        super(chatRoomMemberData.profile);
        this.memberInfo = chatRoomMemberData.member;
    }
}

class ChatApp {
    static #ElementBuilder = class ChatAppElementBuilder {
        constructor() {
            this.ChatAppWindow = $(`
                <div class="chat-app-window">
                    <div class="windows">
                    </div>
                    <div class="side-bar">
                        <div class="avatar-bubble">
                            <div class="toggle-button">
                                <img src="/img/people-fill.svg" class="group-button" />
                            </div>
                        </div>
                        <hr>
                        <div class="users-bubble">
                        </div>
                        <hr>
                        <div class="ai-assistant">
                            <div class="avatar-bubble">
                                <img class="avatar" src="/img/robot.png" alt="avatar">
                            </div>
                        </div>
                    </div>
                </div>
            `);

            this.chatAppSideBar = this.ChatAppWindow.find(".side-bar");
            this.windows = this.ChatAppWindow.find(".windows");
            this.toggleButton = this.chatAppSideBar.find(".toggle-button");
            this.usersBubble = this.chatAppSideBar.find(".users-bubble");
            this.aiAssistantButton = this.chatAppSideBar.find(".ai-assistant");
        }
    };
    constructor(clientUser) {
        this.clientUser = clientUser;

        this.AppElement = new ChatApp.#ElementBuilder();

        this.windows = [];

        if (clientUser != undefined) {
            this.FriendWindow = new FriendWindow(clientUser);
            this.AppElement.windows.append(this.FriendWindow.element);
            this.windows.push(this.FriendWindow)
            this.FriendWindow.hide();
        }

        this.AIAssistantWindow = new AIAssistantWindow(clientUser);
        this.AppElement.windows.append(this.AIAssistantWindow.element);
        this.windows.push(this.AIAssistantWindow)
        this.AIAssistantWindow.hide();
        
        this.AppElement.aiAssistantButton.on("click", () => {
            this.openSidebar()
            for (const window of this.windows) {
                if (window == this.AIAssistantWindow) continue;
                window.hide();
            }
            this.FriendWindow?.hide();
            this.AIAssistantWindow.toggle({
                direction: "right",
                complete: this.autoChatWindow.bind(this)
            });
        })

        this.AppElement.toggleButton.on("click", () => {
            this.openSidebar()
            for (const window of this.windows) {
                if (window == this.FriendWindow) continue;
                window.hide();
            }
            this.AIAssistantWindow.hide()
            this.FriendWindow?.toggle({
                direction: "right",
                complete: this.autoChatWindow.bind(this)
            });
        })

        $.get(`/api/User/ChatRooms`, (resp) => {
            for (const chatRoom of resp) {
                const chatWindow = new ChatWindow(this.clientUser, chatRoom)
                const avatarBubble = chatWindow.WindowElement.sidebarMemberAvatarBubble;
                chatWindow.hide();
                this.windows.push(chatWindow)
                this.AppElement.usersBubble.append(avatarBubble)
                this.AppElement.windows.append(chatWindow.element)
                avatarBubble.on("click", () => {
                    this.openSidebar()
                    for (const window of this.windows) {
                        if (window == chatWindow) continue;
                        window.hide();
                    }
                    chatWindow.toggle({
                        direction: "right",
                        complete: this.autoChatWindow.bind(this)
                    });
                })
            }; 
            this.autoChatWindow();
        })
    }

    autoChatWindow() {
        if (this.windows.every(w => !$(w.element).is(":visible"))) {
            this.AppElement.chatAppSideBar.css({
                height: "120px",
                "border-radius": "5px 5px 5px 5px"
            })
            this.AppElement.ChatAppWindow.css({
                "z-index": 999
            })
        } else {
            this.AppElement.ChatAppWindow.css({
                "z-index": 1001
            })
            this.AppElement.chatAppSideBar.css({
                height: "450px",
                "border-radius": "0 5px 5px 0"
            })
        }
    }

    openSidebar() {
        this.AppElement.chatAppSideBar.css({
            height: "450px",
            "border-radius": "0 5px 5px 0"
        })
    }

    get element() {
        return this.AppElement.ChatAppWindow;
    }
}

class Window {
    
}

class FriendWindow {
    static #ElementBuilder = class FriendWindowElementBuilder {
        constructor() {
            this.friendWindow = $(`
                <div class="friends-window">
                    <div class="search-bar">
                        <div class="input-controls">
                            <input type="text" class="user-input form-control" placeholder="Search" aria-label="Message"
                                aria-describedby="button-addon2">
                        </div>
                    </div>
                    <div class="friends-contener">
                        <div class="status-online">
                            <div class="status-name">
                                <span class="">線上</span>
                                <hr>
                            </div>
                        </div>
                        <div class="status-offline">
                            <div class="status-name">
                                <span class="">離線</span>
                                <hr>
                            </div>
                        </div>
                    </div>
                </div>
            `);

            this.searchInputBar = this.friendWindow.find(".user-input");
            this.onlineFriends = this.friendWindow.find(".status-online");
            this.offlineFriends = this.friendWindow.find(".status-offline");
        }
    };

    constructor(clientUser) {
        this.WindowElement = new FriendWindow.#ElementBuilder();
        this.clientUser = clientUser;
        this.friends;

        $.get(`/api/User/Friends`, (resp) => {
            this.friends = resp.accepted.map((friendData) =>
                new UserFriend(friendData)
            );
        })
            .fail(() => {
                const fallbackFriends = {
                    
                };
                this.clientUserFriends = fallbackFriends.accepted.map((friendData) =>
                    new UserFriend(friendData)
                );
            })
            .always(() => {
                for (const friend of this.friends) {
                    if (friend.isOnline) {
                        this.WindowElement.onlineFriends.append(friend.element);
                    } else {
                        this.WindowElement.offlineFriends.append(friend.element);
                    }
                }
            });
        
        this.WindowElement.searchInputBar.on("input", () => {
            const text = this.WindowElement.searchInputBar.val()
            for (const friend of this.friends) {
                if (text == "") {
                    friend.element.show();
                    continue;
                } 
                if (friend.firstName.includes(text) || friend.lastName.includes(text)) {
                    friend.element.show();
                } else {
                    friend.element.hide();
                }
            }
        });
    }

    get element() {
        return this.WindowElement.friendWindow;
    }

    hide() {
        this.element.hide("slide", { direction: "right" });
    }

    show() {
        this.element.show("slide", { direction: "right" });
    }

    toggle(option) {
        this.element.toggle("slide", option);
    }
}

class ChatWindow {
    static #ElementBuilder = class ChatWindowElementBuilder {
        constructor(member) {
            this.ChatWindow = $(`
                <div class="chat-window ">
                    <div class="chat-window-header">
                        <img class="chat-icon" height="80%" src="${member.avatarUrl}" alt="avatar">
                        <h5 class="chat-header-title">${member.firstName + member.lastName}</h5>
                    </div>
                    <div class="chat-window-body">
                        <div class="message-container"></div>
                    </div>
                    <div class="chat-window-footer">
                        <div class="input-controls"><input type="text" class="user-input form-control" placeholder="Message"
                                aria-label="Message" aria-describedby="button-addon2">
                            <button class="send-msg-btn btn btn-icon btn-primary rounded-circle ms-5" type="button">
                                <svg xmlns="http://www.w3.org/2000/svg" width="30" height="30" fill="currentColor"
                                    class="bi bi-send" viewBox="0 0 30 30">
                                    <path
                                        d="M15.854.146a.5.5 0 0 1 .11.54l-5.819 14.547a.75.75 0 0 1-1.329.124l-3.178-4.995L.643 7.184a.75.75 0 0 1 .124-1.33L15.314.037a.5.5 0 0 1 .54.11ZM6.636 10.07l2.761 4.338L14.13 2.576zm6.787-8.201L1.591 6.602l4.339 2.76z">
                                    </path>
                                </svg>
                            </button>
                        </div>
                    </div>
                </div>
            `);

            this.sidebarMemberAvatarBubble = $(`
                <div class="avatar-bubble">
                    <img class="avatar"
                        src="${member.avatarUrl}" alt="avatar">
                </div>
            `);

            this.chatWindowBody = this.ChatWindow.find(".chat-window-body");
            this.messageContainer = this.ChatWindow.find(".message-container");
            this.messageInputBox = this.ChatWindow.find(".user-input");
            this.sendMessageButton = this.ChatWindow.find(".send-msg-btn");
        }
    };

    constructor(clientUser, chatRoom) {
        this.members = [];

        for (const memberData of chatRoom.members) {
            this.members.push(new ChatRoomMember(memberData));
        }

        $.get(`/api/User/ChatRoomMessages/${chatRoom.chatRoomId}`, (resp) => {
            for (const messageData of resp) {
                const message = this.createMessage(
                    this.members.find(m => m.userID == messageData.senderId),
                    messageData.messageContent,
                    new Date(messageData.createdTime)
                )
                this.messages.push(message);
                this.WindowElement.messageContainer.append(message.element);
                this.scrollToBottom();
            }
        })

        this.WindowElement = new ChatWindow.#ElementBuilder(this.members.filter(m => m.userID != clientUser.userID)[0]);

        this.clientUser = clientUser;

        this.chatRoomID = chatRoom.chatRoomId;

        this.messages = [];

        this.WindowElement.sendMessageButton.on(
            "click",
            this.onSendButtonClick.bind(this),
        );

        this.WindowElement.messageInputBox.on("keydown", (e) => {
            if(e.keyCode == 13) {
                this.onSendButtonClick();
            }
        });
        
        setInterval(() => {
            this.messages.forEach((message) => {
                message.updateTimeText();
            });
        }, 5000);
    }

    get element() {
        return this.WindowElement.ChatWindow;
    }

    get nowTimestramp() {
        return new Date().getTime();
    }

    onSendButtonClick() {
        let inputText = this.WindowElement.messageInputBox.val();
        this.WindowElement.messageInputBox.val("");
        if (inputText.length <= 0) return;

        connection.invoke("SendMessage", this.chatRoomID, inputText).catch(function (err) {
            return console.error(err.tostring());
        });

        

        // $.ajax({
        //     type: "POST",
        //     url: "https://localhost:7233/chat/gpt",
        //     dataType: "JSON",
        //     data: {question: inputText},
        //     success: function (data) {
        //         this.messageContainer.append(
        //             ChatWindow.createMessage(data, this.date.getTime(), false)
        //         );
        //     }
        // })

    }

    createMessage(sender, content, timestramp) {
        return new Message(sender, content, timestramp, this.clientUser);
    }

    scrollToBottom() {
        this.WindowElement.chatWindowBody.scrollTop(this.WindowElement.messageContainer.height());
    }

    hide() {
        this.element.hide("slide", { direction: "right" });
    }

    show() {
        this.element.show("slide", { direction: "right" });
    }

    toggle(option) {
        this.element.toggle("slide", option);
    }
}

class AIAssistantWindow {
    static #ElementBuilder = class AIAssistantWindowElementBuilder {
            constructor() {
                this.AIAssistantWindow = $(`
                    <div class="chat-window">
                        <div class="chat-window-header">
                            <img class="chat-icon" height="80%" src="/img/robot.png" alt="chat-icon">
                            <h5 class="chat-header-title">AI 小幫手</h5>
                        </div>
                        <div class="chat-window-body">
                            <div class="message-container"></div>
                        </div>
                        <div class="chat-window-footer">
                            <div class="input-controls"><input type="text" class="user-input form-control" placeholder="Message"
                                    aria-label="Message" aria-describedby="button-addon2">
                                <button class="send-msg-btn btn btn-icon btn-primary rounded-circle ms-5" type="button">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="30" height="30" fill="currentColor"
                                        class="bi bi-send" viewBox="0 0 30 30">
                                        <path
                                            d="M15.854.146a.5.5 0 0 1 .11.54l-5.819 14.547a.75.75 0 0 1-1.329.124l-3.178-4.995L.643 7.184a.75.75 0 0 1 .124-1.33L15.314.037a.5.5 0 0 1 .54.11ZM6.636 10.07l2.761 4.338L14.13 2.576zm6.787-8.201L1.591 6.602l4.339 2.76z">
                                        </path>
                                    </svg>
                                </button>
                            </div>
                        </div>
                    </div>
                `);

                this.messageContainer = this.AIAssistantWindow.find(".message-container");
                this.messageInputBox = this.AIAssistantWindow.find(".user-input");
                this.sendMessageButton = this.AIAssistantWindow.find(".send-msg-btn");
                this.chatWindowBody = this.AIAssistantWindow.find(".chat-window-body");
            }
        };

    constructor(clientUser) {
        this.WindowElement = new AIAssistantWindow.#ElementBuilder();

        this.clientUser = clientUser;

        this.messages = [];

        this.WindowElement.sendMessageButton.on(
            "click",
            this.onSendButtonClick.bind(this),
        );

        this.WindowElement.messageInputBox.on("keydown", (e) => {
            if(e.keyCode == 13) {
                this.onSendButtonClick();
            }
        });

        this.loadMessageFromLocalStorage();

        setInterval(() => {
            this.messages.forEach((message) => {
                message.updateTimeText();
            });
        }, 5000);
    }

    get element() {
        return this.WindowElement.AIAssistantWindow;
    }

    get nowTimestramp() {
        return new Date().getTime();
    }

    loadMessageFromLocalStorage() {
        var messageData = JSON.parse(localStorage.getItem("AIAssistantMessages"));
        this.messages = [];
        
        if (messageData?.[clientUser.userID]) {
            for (const localStorageMessage of messageData[clientUser.userID]) {
                const sender = localStorageMessage.senderID == 0 ? robot : clientUser;
                const message = new Message(sender, localStorageMessage.content, localStorageMessage.createAt, clientUser)
                this.messages.push(message);

                this.WindowElement.messageContainer.append(message.element);
            }
            this.scrollToBottom()
        }
    }

    saveMessageToLocalStorage() {
        var messageData = JSON.parse(localStorage.getItem("AIAssistantMessages")) || [];

        messageData[clientUser.userID] = [];
        for (const message of this.messages) {
            messageData[clientUser.userID].push({
                senderID: message.sender.userID,
                content: message.content,
                createAt: message.createAt,
                clientUserID: message.clientUserID
            })
        }
        localStorage.setItem("AIAssistantMessages", JSON.stringify(messageData))  
    }

    getMessageContext() {
        let context = [];
        for (const message of this.messages) {
            context.push({
                "Content": message.content,
                "Role": (message.sender.userID == this.clientUser.userID) ? "User" : "Assistant"
            })
        } 
        return context;
    }

    onSendButtonClick() {
        let inputText = this.WindowElement.messageInputBox.val();
        this.WindowElement.messageInputBox.val("");
        if (inputText.length <= 0) return;

        let clientUserMessage = this.createMessage(
            this.clientUser,
            inputText,
            this.nowTimestramp,
        );

        this.messages.push(clientUserMessage);

        this.WindowElement.messageContainer.append(clientUserMessage.element);
        this.WindowElement.chatWindowBody.scrollTop(this.WindowElement.messageContainer.height());

        var message; 
        $.ajax({
            type: "POST",
            url: "/api/Chat/Assistan",
            contentType: 'application/json',
            data: JSON.stringify(this.getMessageContext()),
            success: (data) => {
                message = this.createMessage(robot, data, this.nowTimestramp)
                this.WindowElement.messageContainer.append(
                    message
                );
            }
        })
        .fail((jqXHR, textStatus, errorThrown) => {
            message = this.createMessage(
                robot,
                errorThrown,
                this.nowTimestramp,
            );
        })
        .always((dataOrjqXHR, textStatus, jqXHRorErrorThrown) => {
            this.messages.push(message);
            this.WindowElement.messageContainer.append(message.element);
            this.WindowElement.chatWindowBody.scrollTop(
            this.saveMessageToLocalStorage()
            );
        })
    }

    createMessage(sender, content, timestramp) {
        return new Message(sender, content, timestramp, this.clientUser);
    }

    hide() {
        this.element.hide("slide", { direction: "right" });
    }

    show() {
        this.element.show("slide", { direction: "right" });
    }

    toggle(option) {
        this.element.toggle("slide", option);
    }
}

class Message {
    static #ElementBuilder = class MessageElementBuilder {
        constructor() {
            this.messageBox = $(`<div class="message">`);
            this.messageAvatar = $(`<img class="message-avatar" src="" alt="" />`);
            this.messageBubble = $('<p class="message-bubble">');
            this.messageDate = $('<i class="message-timestramp">');

            this.messageBox.append([
                this.messageAvatar,
                this.messageBubble,
                this.messageDate,
            ]);
        }

        getElement() {
            return this.messageBox;
        }
    };
    constructor(sender, content, createAt, clientUser) {
        this.content = content;
        this.createAt = createAt;
        this.sender = sender;

        this.WindowElement = new Message.#ElementBuilder();

        if (sender.userID == clientUser.userID) {
            this.WindowElement.messageBox.addClass("self-message");
        }

        this.WindowElement.messageAvatar.attr("src", sender.avatarUrl);

        this.WindowElement.messageBubble.text(content);

        this.WindowElement.messageDate.text($.format.prettyDate(this.createAt));
    }

    get element() {
        return this.WindowElement.getElement();
    }

    updateTimeText() {
        this.WindowElement.messageDate.text($.format.prettyDate(this.createAt));
    }
}
