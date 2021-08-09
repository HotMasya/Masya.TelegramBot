import * as Yup from 'yup';

export interface PhoneModel {
  phone: string;
}

export interface CodeModel {
  code: string;
}

export const phoneScheme: Yup.SchemaOf<PhoneModel> = Yup.object().shape({
  phone: Yup.string()
    .required('Phone is required.')
    .matches(/^\+[0-9]{9,13}$/, 'Invalid phone format.'),
});

export const codeScheme: Yup.SchemaOf<CodeModel> = Yup.object().shape({
  phone: Yup.string()
    .required('Code is required.')
    .matches(/^[0-9]{6}$/, 'Invalid code format.'),
});
