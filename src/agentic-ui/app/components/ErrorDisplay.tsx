"use client";

interface ErrorDisplayProps {
  error: Error | string;
  onRetry?: () => void;
  onDismiss?: () => void;
}

/**
 * Error display component with retry functionality
 */
export function ErrorDisplay({ error, onRetry, onDismiss }: ErrorDisplayProps) {
  const errorMessage = typeof error === 'string' ? error : error.message;
  
  return (
    <div 
      className="flex items-start space-x-3 p-4 bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg"
      role="alert"
      aria-live="assertive"
    >
      {/* Error icon */}
      <div className="flex-shrink-0">
        <svg 
          className="w-5 h-5 text-red-600 dark:text-red-400" 
          fill="currentColor" 
          viewBox="0 0 20 20"
          aria-hidden="true"
        >
          <path 
            fillRule="evenodd" 
            d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" 
            clipRule="evenodd" 
          />
        </svg>
      </div>

      {/* Error content */}
      <div className="flex-1">
        <h3 className="text-sm font-medium text-red-800 dark:text-red-200">
          Error
        </h3>
        <p className="mt-1 text-sm text-red-700 dark:text-red-300">
          {errorMessage}
        </p>
        
        {/* Action buttons */}
        {(onRetry || onDismiss) && (
          <div className="mt-3 flex space-x-3">
            {onRetry && (
              <button
                onClick={onRetry}
                className="text-sm font-medium text-red-600 dark:text-red-400 hover:text-red-500 dark:hover:text-red-300 focus:outline-none focus:underline"
              >
                Try again
              </button>
            )}
            {onDismiss && (
              <button
                onClick={onDismiss}
                className="text-sm font-medium text-red-600 dark:text-red-400 hover:text-red-500 dark:hover:text-red-300 focus:outline-none focus:underline"
              >
                Dismiss
              </button>
            )}
          </div>
        )}
      </div>

      {/* Close button */}
      {onDismiss && (
        <button
          onClick={onDismiss}
          className="flex-shrink-0 text-red-600 dark:text-red-400 hover:text-red-500 dark:hover:text-red-300 focus:outline-none"
          aria-label="Dismiss error"
        >
          <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path 
              fillRule="evenodd" 
              d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" 
              clipRule="evenodd" 
            />
          </svg>
        </button>
      )}
    </div>
  );
}
