"use client";

import { useState, useEffect, useRef } from 'react';

interface ChatInputProps {
  onSubmit: (message: string) => void;
  disabled?: boolean;
  placeholder?: string;
}

const MAX_MESSAGE_LENGTH = 4000;
const MAX_LENGTH_BUFFER = 100; // Allow slight overflow for warning display
const RATE_LIMIT_INTERVAL = 2000; // 2 seconds between messages
const RATE_LIMIT_MAX_MESSAGES = 10; // Max 10 messages per minute

/**
 * Chat input component with validation, character limit, and rate limiting
 */
export function ChatInput({ onSubmit, disabled = false, placeholder = "Ask me anything..." }: ChatInputProps) {
  const [message, setMessage] = useState('');
  const [charCount, setCharCount] = useState(0);
  const [isRateLimited, setIsRateLimited] = useState(false);
  const [rateLimitCountdown, setRateLimitCountdown] = useState(0);
  const [messageTimestamps, setMessageTimestamps] = useState<number[]>([]);
  const inputRef = useRef<HTMLTextAreaElement>(null);

  // Update character count when message changes
  useEffect(() => {
    setCharCount(message.length);
  }, [message]);

  // Rate limit countdown timer
  useEffect(() => {
    if (rateLimitCountdown > 0) {
      const timer = setTimeout(() => {
        setRateLimitCountdown(rateLimitCountdown - 100);
      }, 100);
      return () => clearTimeout(timer);
    } else if (isRateLimited) {
      setIsRateLimited(false);
    }
  }, [rateLimitCountdown, isRateLimited]);

  // Clean up old timestamps (older than 1 minute)
  useEffect(() => {
    const interval = setInterval(() => {
      const now = Date.now();
      setMessageTimestamps(prev => prev.filter(ts => now - ts < 60000));
    }, 5000);
    return () => clearInterval(interval);
  }, []);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validate message
    const trimmedMessage = message.trim();
    if (!trimmedMessage || trimmedMessage.length === 0) {
      return;
    }

    // Character limit is already enforced by UI, this is a safety check
    if (trimmedMessage.length > MAX_MESSAGE_LENGTH) {
      // Error is already visible in the UI, just prevent submission
      return;
    }

    // Check rate limiting
    const now = Date.now();
    const recentTimestamps = messageTimestamps.filter(ts => now - ts < 60000);
    
    // Check if too many messages in the last minute
    if (recentTimestamps.length >= RATE_LIMIT_MAX_MESSAGES) {
      setIsRateLimited(true);
      setRateLimitCountdown(5000); // 5 second cooldown
      return;
    }

    // Check if sending too quickly
    const lastTimestamp = recentTimestamps[recentTimestamps.length - 1];
    if (lastTimestamp && now - lastTimestamp < RATE_LIMIT_INTERVAL) {
      setIsRateLimited(true);
      const remaining = RATE_LIMIT_INTERVAL - (now - lastTimestamp);
      setRateLimitCountdown(remaining);
      return;
    }

    // Submit the message
    onSubmit(trimmedMessage);
    setMessage('');
    setMessageTimestamps([...recentTimestamps, now]);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    // Submit on Enter (without Shift)
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSubmit(e);
    }
  };

  const isOverLimit = charCount > MAX_MESSAGE_LENGTH;
  const canSubmit = !disabled && !isRateLimited && charCount > 0 && !isOverLimit;

  return (
    <form onSubmit={handleSubmit} className="chat-input-form w-full">
      <div className="relative">
        <textarea
          ref={inputRef}
          value={message}
          onChange={(e) => setMessage(e.target.value)}
          onKeyDown={handleKeyDown}
          disabled={disabled || isRateLimited}
          placeholder={isRateLimited 
            ? `Rate limited. Wait ${Math.ceil(rateLimitCountdown / 1000)}s...` 
            : placeholder
          }
          className={`w-full px-4 py-3 pr-24 border rounded-lg resize-none focus:outline-none focus:ring-2 
            ${isOverLimit 
              ? 'border-red-500 focus:ring-red-500' 
              : 'border-gray-300 dark:border-gray-600 focus:ring-blue-500'
            } 
            ${isRateLimited ? 'opacity-50 cursor-not-allowed' : ''}
            dark:bg-gray-800 dark:text-white`}
          rows={3}
          maxLength={MAX_MESSAGE_LENGTH + MAX_LENGTH_BUFFER} // Allow buffer for warning display
          aria-label="Chat message input"
          aria-invalid={isOverLimit}
          aria-describedby="char-count"
        />
        
        {/* Character count indicator */}
        <div 
          id="char-count"
          className={`absolute bottom-2 right-16 text-xs
            ${isOverLimit ? 'text-red-500 font-semibold' : 'text-gray-500 dark:text-gray-400'}`}
          aria-live="polite"
        >
          {charCount}/{MAX_MESSAGE_LENGTH}
        </div>

        {/* Submit button */}
        <button
          type="submit"
          disabled={!canSubmit}
          className={`absolute bottom-2 right-2 px-4 py-2 rounded-lg font-medium transition-colors
            ${canSubmit
              ? 'bg-blue-600 text-white hover:bg-blue-700 focus:ring-2 focus:ring-blue-500'
              : 'bg-gray-300 text-gray-500 cursor-not-allowed dark:bg-gray-700 dark:text-gray-500'
            }`}
          aria-label="Send message"
        >
          Send
        </button>
      </div>

      {/* Rate limit warning */}
      {isRateLimited && (
        <div className="mt-2 text-sm text-yellow-600 dark:text-yellow-400" role="alert">
          ⚠️ Please wait {Math.ceil(rateLimitCountdown / 1000)} seconds before sending another message.
        </div>
      )}

      {/* Character limit warning */}
      {isOverLimit && (
        <div className="mt-2 text-sm text-red-600 dark:text-red-400" role="alert">
          ⚠️ Message exceeds maximum length of {MAX_MESSAGE_LENGTH} characters.
        </div>
      )}
    </form>
  );
}
