import { Outlet, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function DashboardLayout() {
  const { logout, user } = useAuth();
  const location = useLocation();

  const isActive = (path: string) => location.pathname === path;

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <Link to="/dashboard" className="text-xl font-bold text-blue-600">MultiVendor Admin</Link>
            </div>
            <div className="flex items-center space-x-4">
              <span className="text-sm text-gray-700">{user?.email}</span>
              <button onClick={logout} className="text-gray-900 hover:text-blue-600 px-3 py-2 rounded-md text-sm font-medium">
                Logout
              </button>
            </div>
          </div>
        </div>
      </header>
      <div className="flex">
        <aside className="w-64 bg-white shadow-sm h-[calc(100vh-4rem)]">
          <nav className="mt-5 px-2 space-y-1">
            <Link to="/dashboard" className={`${isActive('/dashboard') ? 'bg-blue-50 text-blue-600' : 'text-gray-900 hover:bg-gray-50'} group flex items-center px-2 py-2 text-sm font-medium rounded-md`}>
              Dashboard
            </Link>
            <Link to="/admin/categories" className={`${isActive('/admin/categories') ? 'bg-blue-50 text-blue-600' : 'text-gray-900 hover:bg-gray-50'} group flex items-center px-2 py-2 text-sm font-medium rounded-md`}>
              Categories
            </Link>
            <Link to="/dashboard/products" className={`${isActive('/dashboard/products') ? 'bg-blue-50 text-blue-600' : 'text-gray-900 hover:bg-gray-50'} group flex items-center px-2 py-2 text-sm font-medium rounded-md`}>
              Products
            </Link>
            <Link to="/dashboard/orders" className={`${isActive('/dashboard/orders') ? 'bg-blue-50 text-blue-600' : 'text-gray-900 hover:bg-gray-50'} group flex items-center px-2 py-2 text-sm font-medium rounded-md`}>
              Orders
            </Link>
          </nav>
        </aside>
        <main className="flex-1 p-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
