import * as Yup from 'yup';

export interface AuthModel {
  phone: string;
  code?: string;
}

export const authScheme: Yup.SchemaOf<AuthModel> = Yup.object().shape({
  phone: Yup.string()
    .required('Phone is required.')
    .matches(/^\+[0-9]{9,13}$/, 'Invalid phone format.'),

  code: Yup.string().matches(/^[0-9]{6}$/, 'Invalid code format.'),
});
