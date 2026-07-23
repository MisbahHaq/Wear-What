import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { apiClient } from '../../services/api';

interface Product {
  id: string;
  name: string;
  basePrice: number;
  mainImageUrl?: string;
  shopName: string;
}

export default function BrowseProducts() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    (async () => {
      const { data } = await apiClient.get('/products');
      setProducts(data.data || []);
      setLoading(false);
    })();
  }, []);

  if (loading) return <div className="text-center py-12">Loading...</div>;

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <h1 className="text-3xl font-bold text-gray-900 mb-8">Browse Products</h1>
      <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {products.map((product) => (
          <Link key={product.id} to={`/product/${product.id}`} className="group">
            <div className="bg-white rounded-lg shadow-sm hover:shadow-md transition-shadow">
              <div className="aspect-w-1 aspect-h-1 w-full overflow-hidden rounded-lg bg-gray-200">
                {product.mainImageUrl ? (
                  <img src={product.mainImageUrl} alt={product.name} className="h-64 w-full object-cover object-center group-hover:opacity-90" />
                ) : (
                  <div className="h-64 w-full bg-gray-100 flex items-center justify-center text-gray-400">No Image</div>
                )}
              </div>
              <div className="p-4">
                <h3 className="text-sm font-medium text-gray-900">{product.name}</h3>
                <p className="mt-1 text-lg font-semibold text-gray-900">${product.basePrice}</p>
                <p className="mt-1 text-sm text-gray-500">{product.shopName}</p>
              </div>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
