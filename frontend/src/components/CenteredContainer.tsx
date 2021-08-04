import { Container, createStyles, Theme, withStyles } from '@material-ui/core';

const CenteredContainer = withStyles((theme: Theme) =>
  createStyles({
    root: {
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      textAlign: 'center',
      height: '100%',
      [theme.breakpoints.down('sm')]: {
        padding: 0,
      },
    },
  }),
)(Container);

export default CenteredContainer;
