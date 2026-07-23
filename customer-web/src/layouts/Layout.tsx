import { Outlet, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function Layout() {
  const { logout, isAuthenticated } = useAuth();

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <Link to="/" className="text-xl font-bold text-blue-600">
                MultiVendor
              </Link>
              <nav className="ml-8 flex space-x-4">
                <Link to="/" className="text-gray-900 hover:text-blue-600 px-3 py-2 rounded-md text-sm font-medium">
                  Browse
                </Link>
                {isAuthenticated && (
                  <Link to="/orders" className="text-gray-900 hover:text-blue-600 px-3 py-2 rounded-md text-sm font-medium">
                    My Orders
                  </Link>
                )}
              </nav>
            </div>
            <div className="flex items-center space-x-4">
              <Link to="/cart" className="text-gray-900 hover:text-blue-600 px-3 py-2 rounded-md text-sm font-medium">
                Cart
              </Link>
              {isAuthenticated ? (
                <button onClick={logout} className="text-gray-900 hover:text-blue-600 px-3 py-2 rounded-md text-sm font-medium">
                  Logout
                </button>
              ) : (
                <Link to="/auth/login" className="bg-blue-600 text-white hover:bg-blue-700 px-4 py-2 rounded-md text-sm font-medium">
                  Sign In
                </Link>
              )}
            </div>
          </div>
        </div>
      </header>
      <main>
        <Outlet />
      </main>
    </div>
  );
}
