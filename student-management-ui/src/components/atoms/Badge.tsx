import { cn } from '@/lib/utils';

interface BadgeProps {
  label: string;
  variant?: 'green' | 'yellow' | 'red' | 'gray' | 'blue';
}

const variantClasses: Record<string, string> = {
  green: 'bg-green-100 text-green-800',
  yellow: 'bg-yellow-100 text-yellow-800',
  red: 'bg-red-100 text-red-800',
  gray: 'bg-gray-100 text-gray-700',
  blue: 'bg-blue-100 text-blue-800',
};

export function Badge({ label, variant = 'gray' }: BadgeProps) {
  return (
    <span
      className={cn(
        'inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium',
        variantClasses[variant]
      )}
    >
      {label}
    </span>
  );
}

export function paymentStatusVariant(status: string): BadgeProps['variant'] {
  switch (status.toLowerCase()) {
    case 'paid':
      return 'green';
    case 'pending':
      return 'yellow';
    case 'overdue':
      return 'red';
    default:
      return 'gray';
  }
}
