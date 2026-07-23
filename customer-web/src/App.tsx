import { Routes, Route, Navigate } from 'react-router-dom';
import { useAuth } from './context/AuthContext';
import Layout from './layouts/Layout';
import Login from './pages/auth/Login';
import Register from './pages/auth/Register';
import BrowseProducts from './pages/shop/BrowseProducts';
import ProductDetail from './pages/shop/ProductDetail';
import Cart from './pages/customer/Cart';
import Orders from './pages/customer/Orders';

export default function App() {
  const { isAuthenticated, user } = useAuth();

  return (
    <Routes>
      <Route path="/" element={<Layout />}>
        <Route index element={<BrowseProducts />} />
        <Route path="product/:id" element={<ProductDetail />} />
        <Route path="cart" element={<Cart />} />
        <Route path="orders" element={<Orders />} />
        <Route path="become-seller" element={<Navigate to="/seller" replace />} />

        <Route
          path="auth/login"
          element={isAuthenticated ? <Navigate to="/" replace /> : <Login />}
        />
        <Route
          path="auth/register"
          element={isAuthenticated ? <Navigate to="/" replace /> : <Register />}
        />

        <Route
          path="seller/*"
          element={isAuthenticated && user?.roles.includes('ShopOwner') ? 
            <Navigate to="/seller/dashboard" replace /> : 
            <Navigate to="/auth/login" replace />}
        />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
