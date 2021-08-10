import { Epic } from 'redux-observable';
import { isActionOf } from 'typesafe-actions';
import RootState from '../reducers';
import * as actions from '../actions';
import { of } from 'rxjs';
import { filter, catchError, switchMap, mapTo, map } from 'rxjs/operators';
import { ajax } from 'rxjs/ajax';
import * as endpoints from '../../routing/endpoints';
import { TokenModel } from 'src/models/TokenModel';
import { RootAction } from '..';

export const phoneEpic: Epic<RootAction, RootAction, RootState> = (action$) =>
    action$.pipe(
        filter(isActionOf(actions.checkPhone)),
        switchMap(action =>
            ajax({
                url: endpoints.apiEndpoints.checkPhone,
                method: 'POST',
                body: {
                    phoneNumber: action.payload
                }
            }).pipe(
                mapTo(actions.checkPhoneSuccess()),
                catchError(err => {
                    return of(actions.checkPhoneFailure(err.xhr.response));
                })
            )
        ),
    );

export const codeEpic: Epic<RootAction, RootAction, RootState> = (action$) =>
    action$.pipe(
        filter(isActionOf(actions.checkCode)),
        switchMap(action =>
            ajax<TokenModel>({
                url: endpoints.apiEndpoints.checkCode,
                method: 'POST',
                body: {
                    code: action.payload.code,
                    phoneNumber: action.payload.phone,
                }
            }).pipe(
                map(data => {
                    var token = data.response;
                    localStorage.setItem('x-access-token', token.token);
                    return actions.checkCodeSuccess(token);
                }),
                catchError(err => {
                    return of(actions.checkCodeFailure(err.xhr.response));
                })
            )
        ),
    );
