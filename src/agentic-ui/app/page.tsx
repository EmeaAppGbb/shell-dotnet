"use client";
import { CopilotSidebar } from "@copilotkit/react-ui";
import { useHumanInTheLoop } from "@copilotkit/react-core";
import { useState } from "react";

export default function Page() {
  const [imageContent, setImageContent] = useState<string | null>(null);
  const [textContent, setTextContent] = useState<string | null>(null);
  // Human-in-the-loop tool for text content approval
  useHumanInTheLoop({
    name: "approve_copyright_command",
    description: "Ask the user to approve the generated text content",
    parameters: [
      {
        name: "copyright",
        type: "string",
        description: "The text content to review and approve",
        required: true,
      },
    ],
    render: ({ args, respond }) => {
      if (!respond) return <></>;
      
      return (
        <div className="p-4 mb-4 bg-yellow-50 dark:bg-yellow-900/20 border-2 border-yellow-300 dark:border-yellow-700 rounded-lg shadow-md">
          <p className="font-semibold text-yellow-900 dark:text-yellow-100 mb-2">
            📝 Text Content Approval Required
          </p>
          <p className="text-sm text-yellow-800 dark:text-yellow-200 mb-3">
            Please review the generated text content below and approve or deny:
          </p>
          <div className="bg-gray-100 dark:bg-gray-900 p-3 rounded-lg mb-3">
            <pre className="text-gray-800 dark:text-gray-200 text-sm font-mono whitespace-pre-wrap break-words">
              {args.copyright}
            </pre>
          </div>
          <div className="flex gap-3">
            <button 
              onClick={() => {
                setTextContent(args.copyright);
                respond("text-approved");
              }}
              className="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 text-white font-semibold rounded-lg transition-colors shadow-md hover:shadow-lg"
            >
              ✓ Approve
            </button>
            <button 
              onClick={() => {
                setTextContent(null);
                respond("text-rejected");
              }}
              className="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 text-white font-semibold rounded-lg transition-colors shadow-md hover:shadow-lg"
            >
              ✗ Deny
            </button>
          </div>
        </div>
      );
    },
  });

  // Human-in-the-loop tool for image content approval
  useHumanInTheLoop({
    name: "approve_design_command",
    description: "Ask the user to approve the generated image content",
     parameters: [
      {
        name: "design",
        type: "string",
        description: "The image content to review and approve",
        required: true,
      },
    ],
    render: ({ args, respond }) => {
      if (!respond) return <></>;
      
      return (
        <div className="p-4 mb-4 bg-blue-50 dark:bg-blue-900/20 border-2 border-blue-300 dark:border-blue-700 rounded-lg shadow-md">
          <p className="font-semibold text-blue-900 dark:text-blue-100 mb-2">
            🖼️ Image Content Approval Required
          </p>
          <p className="text-sm text-blue-800 dark:text-blue-200 mb-3">
            Please review the generated image below and approve or reject:
          </p>
          <div className="bg-gray-100 dark:bg-gray-900 p-3 rounded-lg mb-3">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img 
              src={args.design} 
              alt="Generated image for approval" 
              className="max-w-full h-auto rounded shadow-lg mx-auto"
            />
          </div>
          <div className="flex gap-3">
            <button 
              onClick={() => {
                setImageContent(args.design);
                respond("image-approved");
              }}
              className="flex-1 px-4 py-2 bg-green-600 hover:bg-green-700 text-white font-semibold rounded-lg transition-colors shadow-md hover:shadow-lg"
            >
              ✓ Approve
            </button>
            <button 
              onClick={() => {
                setImageContent(null);
                respond("image-rejected");
              }}
              className="flex-1 px-4 py-2 bg-red-600 hover:bg-red-700 text-white font-semibold rounded-lg transition-colors shadow-md hover:shadow-lg"
            >
              ✗ Reject
            </button>
          </div>
        </div>
      );
    },
  });

  return (
    <CopilotSidebar
      defaultOpen={true}
      labels={{
        title: "AI Assistant",
        initial: "Hi! 👋 I'm your AI assistant. How can I help you today?",
        placeholder: "Ask me anything...",
      }}
      instructions="You are a helpful AI assistant. Provide clear, concise, and accurate responses to user queries."
    >
      <main className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800">
        <div className="container mx-auto px-4 py-12 max-w-4xl">
          {textContent && (
            <div className="mb-8 bg-white dark:bg-gray-800 rounded-2xl shadow-2xl p-6 border-4 border-yellow-500 dark:border-yellow-400">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-2xl font-bold text-yellow-900 dark:text-yellow-100">
                  📝 Generated Text Content
                </h2>
                <span className="px-3 py-1 bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200 rounded-full text-sm font-semibold">
                  ✓ Approved
                </span>
              </div>
              <div className="bg-gray-100 dark:bg-gray-900 p-4 rounded-lg">
                <pre className="text-green-600 dark:text-green-400 text-sm font-mono whitespace-pre-wrap break-words">
                  {textContent}
                </pre>
              </div>
              <p className="mt-4 text-sm text-gray-600 dark:text-gray-400 text-center">
                This text content has been approved
              </p>
            </div>
          )}
          {imageContent && (
            <div className="mb-8 bg-white dark:bg-gray-800 rounded-2xl shadow-2xl p-6 border-4 border-blue-500 dark:border-blue-400">
              <div className="flex items-center justify-between mb-4">
                <h2 className="text-2xl font-bold text-blue-900 dark:text-blue-100">
                  🖼️ Generated Image Content
                </h2>
                <span className="px-3 py-1 bg-green-100 dark:bg-green-900 text-green-800 dark:text-green-200 rounded-full text-sm font-semibold">
                  ✓ Approved
                </span>
              </div>
              <div className="bg-gray-100 dark:bg-gray-900 p-4 rounded-lg">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img 
                  src={imageContent} 
                  alt="Generated image content" 
                  className="max-w-full h-auto rounded shadow-lg mx-auto"
                />
              </div>
              <p className="mt-4 text-sm text-gray-600 dark:text-gray-400 text-center">
                This image content has been approved
              </p>
            </div>
          )}
          <div className="text-center space-y-6">
            <div className="inline-block p-3 bg-blue-100 dark:bg-blue-900 rounded-full mb-4">
              <svg 
                className="w-12 h-12 text-blue-600 dark:text-blue-400" 
                fill="none" 
                stroke="currentColor" 
                viewBox="0 0 24 24"
              >
                <path 
                  strokeLinecap="round" 
                  strokeLinejoin="round" 
                  strokeWidth={2} 
                  d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" 
                />
              </svg>
            </div>
            <h1 className="text-5xl font-bold text-gray-900 dark:text-white">
              Agentic Shell
            </h1>
            <p className="text-xl text-gray-600 dark:text-gray-300 max-w-2xl mx-auto">
              An intelligent assistant powered by AI. Open the sidebar to start chatting.
            </p>
          </div>

          <div className="mt-16 grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 hover:shadow-xl transition-shadow">
              <div className="w-12 h-12 bg-green-100 dark:bg-green-900 rounded-lg flex items-center justify-center mb-4">
                <svg className="w-6 h-6 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                Fast Responses
              </h3>
              <p className="text-gray-600 dark:text-gray-300">
                Get instant answers with AI-powered intelligence. Input validation ensures quality messages under 4000 characters.
              </p>
            </div>

            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 hover:shadow-xl transition-shadow">
              <div className="w-12 h-12 bg-purple-100 dark:bg-purple-900 rounded-lg flex items-center justify-center mb-4">
                <svg className="w-6 h-6 text-purple-600 dark:text-purple-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4" />
                </svg>
              </div>
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                Rich Content Support
              </h3>
              <p className="text-gray-600 dark:text-gray-300">
                Advanced AI with markdown rendering and code syntax highlighting for technical content.
              </p>
            </div>

            <div className="bg-white dark:bg-gray-800 rounded-lg shadow-lg p-6 hover:shadow-xl transition-shadow">
              <div className="w-12 h-12 bg-orange-100 dark:bg-orange-900 rounded-lg flex items-center justify-center mb-4">
                <svg className="w-6 h-6 text-orange-600 dark:text-orange-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
                </svg>
              </div>
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-2">
                Reliable & Monitored
              </h3>
              <p className="text-gray-600 dark:text-gray-300">
                Built-in health checks, error handling, and rate limiting ensure a stable experience.
              </p>
            </div>
          </div>
        </div>
      </main>
    </CopilotSidebar>
  );
}