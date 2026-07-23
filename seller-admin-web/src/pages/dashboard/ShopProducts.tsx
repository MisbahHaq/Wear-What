import { useState, useEffect } from 'react';
import { apiClient } from '../../services/api';

export default function ShopProducts() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      const { data } = await apiClient.get('/products?shopId=me');
      setProducts(data.data || []);
      setLoading(false);
    })();
  }, []);

  if (loading) return <div>Loading...</div>;

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">My Products</h1>
      <div className="bg-white shadow-sm rounded-lg p-6">
        <p className="text-gray-600">Manage your products here. Coming soon.</p>
      </div>
    </div>
  );
}
