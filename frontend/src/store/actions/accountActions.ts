import { User } from '../../models/User';
import { createStandardAction } from 'typesafe-actions';
import { AccountActionTypes } from '../action-types';
import { TokenModel } from '../../models/TokenModel';
import { AuthModel } from '../../models/AuthModel';

export const checkPhone = createStandardAction(AccountActionTypes.CHECK_PHONE)<string>();
export const checkPhoneSuccess = createStandardAction(AccountActionTypes.CHECK_PHONE_SUCCESS)();
export const checkPhoneFailure = createStandardAction(AccountActionTypes.CHECK_PHONE_FAILURE)<Error>();
export const checkCode = createStandardAction(AccountActionTypes.CHECK_CODE)<AuthModel>();
export const checkCodeSuccess = createStandardAction(AccountActionTypes.CHECK_CODE_SUCCESS)<TokenModel>();
export const checkCodeFailure = createStandardAction(AccountActionTypes.CHECK_CODE_FAILURE)<Error>();
export const getUser = createStandardAction(AccountActionTypes.GET_USER)<TokenModel>();
export const setUser = createStandardAction(AccountActionTypes.SET_USER)<User>();
export const userError = createStandardAction(AccountActionTypes.ERROR)<Error>();

