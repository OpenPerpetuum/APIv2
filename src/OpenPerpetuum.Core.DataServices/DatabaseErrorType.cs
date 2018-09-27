namespace OpenPerpetuum.Core.DataServices
{
	// This enum should contain the CUSTOM error type/code mapping
	// e.g. CharacterNameExists could be a violation of UQ_Character_Name with code 110001
	// but the error code 2601 (MS SQL Unique Index Violation) is also mapped as a common error type etc.
	// Just helps make things easier to understand in the code when you're reading DatabaseErrorType.CharacterNameExists vs SqlException.ErrorCode = 110001
	public enum DatabaseErrorType
	{
		UniqueIndexViolation = 2601, // Violation of Unique index
		UniqueConstraintViolation = 2627, // Violation of Primary Key
		UnspecifiedFatalError
	}
}
