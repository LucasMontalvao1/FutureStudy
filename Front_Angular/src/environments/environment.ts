const apiUrl = 'https://localhost:7281/api/v1';

export const environment = {
  production: false,
  apiUrl: apiUrl,
  endpoints: {
    login: `${apiUrl}/login`
  },
};