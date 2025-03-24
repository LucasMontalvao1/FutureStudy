const apiUrl = 'https://localhost:7281/api/v1';

export const environment = {
  production: false,
  apiUrl: apiUrl,
  endpoints: {
    login: `${apiUrl}/login`,
    sessaoestudo: `${apiUrl}/SessoesEstudo`,
    metas: `${apiUrl}/metas`,
    anotacoes: `${apiUrl}/anotacoes`,
  },
};



