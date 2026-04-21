import { NavLink, Outlet } from 'react-router-dom';

const NAV = [
  { to: '/', label: '💬 Sohbet', end: true },
  { to: '/students', label: '🎓 Öğrenciler', end: false },
  { to: '/documents', label: '📄 Belgeler', end: false },
];

export function Layout() {
  return (
    <div className="flex h-screen overflow-hidden bg-gray-50">
      {/* Persistent sidebar */}
      <aside className="flex w-56 shrink-0 flex-col gap-1 border-r border-gray-200 bg-white px-3 py-6">
        <p className="mb-4 px-2 text-xs font-semibold uppercase tracking-widest text-gray-400">
          Menü
        </p>
        {NAV.map(({ to, label, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            className={({ isActive }) =>
              `rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                isActive
                  ? 'bg-indigo-50 text-indigo-700'
                  : 'text-gray-600 hover:bg-gray-100 hover:text-gray-900'
              }`
            }
          >
            {label}
          </NavLink>
        ))}
      </aside>

      {/* Page content */}
      <main className="flex flex-1 flex-col overflow-hidden">
        <Outlet />
      </main>
    </div>
  );
}
