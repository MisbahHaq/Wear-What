import { useParams } from 'react-router-dom';

export default function ProductDetail() {
  const { id } = useParams();
  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="bg-white rounded-lg shadow-sm p-6">
        <h1 className="text-2xl font-bold text-gray-900">Product {id}</h1>
        <p className="mt-4 text-gray-600">Product details coming soon.</p>
      </div>
    </div>
  );
}
