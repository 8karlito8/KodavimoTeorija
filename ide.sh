#!/bin/bash

SESSION_NAME="$1"

tmux kill-session -t "$SESSION_NAME" 2>/dev/null
tmux new-session -d -s "$SESSION_NAME" -n server

# Create additional windows
tmux new-window -t "$SESSION_NAME" -n client
tmux new-window -t "$SESSION_NAME" -n run_server
tmux new-window -t "$SESSION_NAME" -n run_client
tmux new-window -t "$SESSION_NAME" -n claude

# Send commands to each window
tmux send-keys -t "$SESSION_NAME:server" "cd server && nvim" Enter
tmux send-keys -t "$SESSION_NAME:client" "cd client && nvim" Enter
tmux send-keys -t "$SESSION_NAME:run_server" "cd server" Enter
tmux send-keys -t "$SESSION_NAME:run_client" "cd client" Enter
tmux send-keys -t "$SESSION_NAME:claude" "claude --resume" Enter

# Attach to the session
tmux attach-session -t "$SESSION_NAME"
