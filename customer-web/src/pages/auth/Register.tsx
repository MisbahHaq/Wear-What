import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { apiClient } from '../../services/api';

export default function Register() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const { data } = await apiClient.post('/auth/register', {
        email,
        password,
        confirmPassword: password,
        firstName,
        lastName,
        role: 'Customer',
      });
      login(data.token, { userId: data.userId, email: data.email, firstName: data.firstName, lastName: data.lastName, roles: data.roles, shopId: data.shopId });
      navigate('/');
    } catch {
      alert('Registration failed');
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">Create your account</h2>
        </div>
        <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
          <div className="space-y-4">
            <input type="text" required value={firstName} onChange={(e) => setFirstName(e.target.value)} placeholder="First Name" className="relative block w-full px-3 py-2 border border-gray-300 text-gray-900 rounded-md focus:ring-blue-500 focus:border-blue-500 sm:text-sm" />
            <input type="text" required value={lastName} onChange={(e) => setLastName(e.target.value)} placeholder="Last Name" className="relative block w-full px-3 py-2 border border-gray-300 text-gray-900 rounded-md focus:ring-blue-500 focus:border-blue-500 sm:text-sm" />
            <input type="email" required value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" className="relative block w-full px-3 py-2 border border-gray-300 text-gray-900 rounded-md focus:ring-blue-500 focus:border-blue-500 sm:text-sm" />
            <input type="password" required value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Password" className="relative block w-full px-3 py-2 border border-gray-300 text-gray-900 rounded-md focus:ring-blue-500 focus:border-blue-500 sm:text-sm" />
          </div>
          <div>
            <button type="submit" className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500">
              Sign up
            </button>
          </div>
          <div className="text-sm text-center">
            Already have an account? <Link to="/auth/login" className="font-medium text-blue-600 hover:text-blue-500">Sign in</Link>
          </div>
        </form>
      </div>
    </div>
  );
}
