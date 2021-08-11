export const host = 'localhost:50001';
export const apiHost = 'https://masya.azurewebsites.net/';

export const endpoints = {
  auth: host + '/auth',
};

export const apiEndpoints = {
  checkPhone: apiHost + 'auth/phone',
  checkCode: apiHost + 'auth/code',
  getUserInfo: apiHost + 'api/user/me',
}
