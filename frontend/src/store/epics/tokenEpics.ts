// import { Epic } from "redux-observable";
// import { RootAction } from "..";
// import RootState from "../reducers";
// import { actions } from "..";
// import { isActionOf } from "typesafe-actions";
// import { filter, switchMap } from "rxjs/operators";
// import { ajax } from "rxjs/ajax";
// import { apiEndpoints } from "src/routing/endpoints";
// import { timer } from "rxjs";

// export const refreshToken: Epic<RootAction, RootAction, RootState> = (action$, state) => action$.pipe(
//     filter(isActionOf(actions.tokenRefresh)),
//     switchMap((action) => 
//     timer(0, 10 * 60 * 1000))
// )