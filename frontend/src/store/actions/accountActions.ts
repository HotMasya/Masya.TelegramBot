import { User } from '../../models/User';
import { createStandardAction } from 'typesafe-actions';
import { AccountActionTypes } from '../action-types';
import { Token } from '../../models/Token';

export const checkPhone = createStandardAction(AccountActionTypes.CHECK_PHONE)<string>();
export const checkPhoneSuccess = createStandardAction(AccountActionTypes.CHECK_PHONE_SUCCESS)();
export const checkPhoneFailure = createStandardAction(AccountActionTypes.CHECK_PHONE_FAILURE)<Error>();
export const checkCode = createStandardAction(AccountActionTypes.CHECK_CODE)<string>();
export const checkCodeSuccess = createStandardAction(AccountActionTypes.CHECK_CODE_SUCCESS)<Token>();
export const checkCodeFailure = createStandardAction(AccountActionTypes.CHECK_CODE_FAILURE)<Error>();
export const setUser = createStandardAction(AccountActionTypes.SET_USER)<User>();
export const userError = createStandardAction(AccountActionTypes.ERROR)<Error>();
