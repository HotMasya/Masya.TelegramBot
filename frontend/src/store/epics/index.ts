import * as accountEpics from './accountEpics';
import { combineEpics } from 'redux-observable';

const rootEpic = combineEpics(accountEpics.codeEpic, accountEpics.phoneEpic);

export default rootEpic;
