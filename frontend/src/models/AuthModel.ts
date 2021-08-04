import * as Yup from 'yup';

export interface PhoneModel {
  phone: string;
}

export const phoneScheme: Yup.SchemaOf<PhoneModel> = Yup.object().shape({
  phone: Yup.string()
    .required('Phone is required.')
    .matches(/^\+[0-9]{9,13}$/, 'Invalid phone format.'),
});
