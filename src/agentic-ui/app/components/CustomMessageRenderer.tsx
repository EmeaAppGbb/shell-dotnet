"use client";

import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import rehypeHighlight from 'rehype-highlight';
import 'highlight.js/styles/github-dark.css';
import type { Components } from 'react-markdown';

interface CustomMessageRendererProps {
  content: string;
  role: 'user' | 'assistant' | 'system';
}

/**
 * Custom message renderer component that supports markdown rendering
 * with code syntax highlighting for AI chat messages.
 */
export function CustomMessageRenderer({ content, role }: CustomMessageRendererProps) {
  // For user messages, just display plain text
  if (role === 'user') {
    return (
      <div className="user-message">
        <p className="text-gray-900 dark:text-white">{content}</p>
      </div>
    );
  }

  // For assistant/system messages, render markdown with code highlighting
  const components: Components = {
    // Customize code block rendering
    code: (props) => {
      const { children, className, ...rest } = props;
      const match = /language-(\w+)/.exec(className || '');
      const isInline = !match;
      
      return isInline ? (
        <code className="bg-gray-100 dark:bg-gray-800 px-1 py-0.5 rounded text-sm" {...rest}>
          {children}
        </code>
      ) : (
        <code className={className} {...rest}>
          {children}
        </code>
      );
    },
    // Style links
    a: (props) => {
      const { children, ...rest } = props;
      return (
        <a
          className="text-blue-600 dark:text-blue-400 hover:underline"
          target="_blank"
          rel="noopener noreferrer"
          {...rest}
        >
          {children}
        </a>
      );
    },
  };

  return (
    <div className="assistant-message prose dark:prose-invert max-w-none">
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        rehypePlugins={[rehypeHighlight]}
        components={components}
      >
        {content}
      </ReactMarkdown>
    </div>
  );
}
