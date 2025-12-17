# Chat Interface Components

This directory contains reusable components for the AI chat interface with enhanced features for input validation, markdown rendering, and error handling.

## Components

### ChatInput

**File:** `ChatInput.tsx`

**Purpose:** Enhanced chat input component with validation, character limits, and rate limiting.

**Features:**
- Character limit enforcement (4000 characters)
- Real-time character counter with visual feedback
- Rate limiting (10 messages per minute, 2 seconds between messages)
- Visual warnings for exceeded limits
- ARIA labels for accessibility
- Enter key to submit (Shift+Enter for new line)

**Usage:**
```tsx
import { ChatInput } from './components/ChatInput';

function MyChat() {
  const handleSubmit = (message: string) => {
    console.log('Sending message:', message);
    // Send to API
  };

  return (
    <ChatInput
      onSubmit={handleSubmit}
      disabled={false}
      placeholder="Type your message..."
    />
  );
}
```

**Props:**
- `onSubmit: (message: string) => void` - Callback when message is submitted
- `disabled?: boolean` - Disable input while processing
- `placeholder?: string` - Placeholder text

---

### CustomMessageRenderer

**File:** `CustomMessageRenderer.tsx`

**Purpose:** Renders chat messages with markdown support and code syntax highlighting.

**Features:**
- Full markdown rendering (via react-markdown)
- GitHub Flavored Markdown support (via remark-gfm)
- Code syntax highlighting (via rehype-highlight)
- Custom styling for code blocks, links, and inline code
- Dark mode support

**Usage:**
```tsx
import { CustomMessageRenderer } from './components/CustomMessageRenderer';

function ChatMessage({ text, role }) {
  return (
    <CustomMessageRenderer
      content={text}
      role={role}
    />
  );
}
```

**Props:**
- `content: string` - The message content (supports markdown)
- `role: 'user' | 'assistant' | 'system'` - Message sender role

**Markdown Support:**
- Headers, lists, tables
- Bold, italic, strikethrough
- Code blocks with syntax highlighting
- Inline code
- Links (open in new tab)
- Blockquotes

---

### TypingIndicator

**File:** `TypingIndicator.tsx`

**Purpose:** Visual indicator shown while AI is generating a response.

**Features:**
- Animated dots
- "AI is thinking..." message
- Accessible label for screen readers

**Usage:**
```tsx
import { TypingIndicator } from './components/TypingIndicator';

function Chat() {
  const [isLoading, setIsLoading] = useState(false);

  return (
    <div>
      {isLoading && <TypingIndicator />}
    </div>
  );
}
```

**Props:** None

---

### ErrorDisplay

**File:** `ErrorDisplay.tsx`

**Purpose:** Displays error messages with retry and dismiss functionality.

**Features:**
- User-friendly error display
- Retry button (optional)
- Dismiss button (optional)
- Error icon with visual feedback
- ARIA live region for screen readers
- Dark mode support

**Usage:**
```tsx
import { ErrorDisplay } from './components/ErrorDisplay';

function Chat() {
  const [error, setError] = useState<Error | null>(null);

  const handleRetry = () => {
    setError(null);
    // Retry logic
  };

  return (
    <div>
      {error && (
        <ErrorDisplay
          error={error}
          onRetry={handleRetry}
          onDismiss={() => setError(null)}
        />
      )}
    </div>
  );
}
```

**Props:**
- `error: Error | string` - The error to display
- `onRetry?: () => void` - Optional retry callback
- `onDismiss?: () => void` - Optional dismiss callback

---

## Styling

All components use Tailwind CSS with dark mode support. The styling is consistent with the application's design system.

### Required Dependencies

```json
{
  "react-markdown": "^10.1.0",
  "remark-gfm": "^4.0.1",
  "rehype-highlight": "^7.0.2"
}
```

### CSS Requirements

Include highlight.js CSS for syntax highlighting:

```tsx
import 'highlight.js/styles/github-dark.css';
```

---

## Accessibility

All components follow WCAG 2.1 Level AA guidelines:

- Proper ARIA labels and roles
- Keyboard navigation support
- Screen reader announcements for dynamic content
- Visual focus indicators
- Color contrast compliance

---

## Best Practices

1. **ChatInput**: Always validate messages on both client and server side
2. **CustomMessageRenderer**: Sanitize user input before rendering (react-markdown handles this)
3. **ErrorDisplay**: Provide actionable error messages that guide users
4. **TypingIndicator**: Show during async operations to provide feedback

---

## Future Enhancements

Potential improvements for these components:

- Voice input support for ChatInput
- Image upload and rendering in CustomMessageRenderer
- More granular error types in ErrorDisplay
- Customizable typing indicator animations
- Conversation export functionality
