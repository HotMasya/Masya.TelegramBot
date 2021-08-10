import * as accountActions from './accountActions';
import { ActionType } from 'typesafe-actions';

type RootAction = ActionType<typeof accountActions>;

export default RootAction;
export * from './accountActions';