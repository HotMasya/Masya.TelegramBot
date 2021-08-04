import { Box, createStyles, Theme, withStyles } from '@material-ui/core';

const CenteredBox = withStyles((theme: Theme) =>
  createStyles({
    root: {
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      textAlign: 'center',
    },
  }),
)(Box);

export default CenteredBox;
