using _440DocumentManagement.Models.ApiDatabase;
using System.Collections.Generic;

namespace _440DocumentManagement.Helpers
{
	public class ApiDatabaseTableReferences
	{
		public static List<ApiDatabaseTable> Instance { get; } = new List<ApiDatabaseTable>()
		{
			new ApiDatabaseTable()
			{
				TableName = "905-SourceSystemScraper",
				PrimaryKey = "sys_scrape_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_source_system_id",
						RelatedTableName = "customer_source_systems",
						RelatedPrimaryKey = "customer_source_sys_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "source_sys_type_id",
						RelatedTableName = "source_system_types",
						RelatedPrimaryKey = "source_sys_type_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "app_transaction_log",
				PrimaryKey = "log_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "document_id",
						RelatedTableName = "project_documents",
						RelatedPrimaryKey = "doc_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "file_id",
						RelatedTableName = "files",
						RelatedPrimaryKey = "file_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "notification_id",
						RelatedTableName = "user_notifications",
						RelatedPrimaryKey = "user_notification_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "submission_id",
						RelatedTableName = "project_submissions",
						RelatedPrimaryKey = "project_submission_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "device_id",
						RelatedTableName = "user_devices",
						RelatedPrimaryKey = "user_device_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "available_settings",
				PrimaryKey = "setting_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "blocked_users",
				PrimaryKey = "blocked_user_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "calendar_events",
				PrimaryKey = "calendar_event_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "calendar_events_old",
				PrimaryKey = "calendar_event_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "company_offices",
				PrimaryKey = "company_office_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "customer_attributes",
				PrimaryKey = "customer_attribute_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "system_attribute_id",
						RelatedTableName = "system_attributes",
						RelatedPrimaryKey = "system_attribute_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "customer_companies",
				PrimaryKey = "company_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "customer_contacts",
				PrimaryKey = "contact_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "company_id",
						RelatedTableName = "customer_companies",
						RelatedPrimaryKey = "company_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "customer_destinations",
				PrimaryKey = "destination_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "customer_office_companies",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "customer_office_contacts",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "customer_settings",
				PrimaryKey = "customer_setting_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "customer_source_systems",
				PrimaryKey = "customer_source_sys_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "source_sys_type_id",
						RelatedTableName = "source_system_types",
						RelatedPrimaryKey = "source_sys_type_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "customer_subscriptions",
				PrimaryKey = "customer_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "customers",
				PrimaryKey = "customer_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "data_source_fields",
				PrimaryKey = "data_source_field_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_attribute_id",
						RelatedTableName = "customer_attributes",
						RelatedPrimaryKey = "customer_attribute_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "data_source_filter_templates",
				PrimaryKey = "data_source_filter_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "data_source_id",
						RelatedTableName = "data_sources",
						RelatedPrimaryKey = "data_source_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "data_sources",
				PrimaryKey = "data_source_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "data_view_field_settings",
				PrimaryKey = "data_view_field_setting_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "data_view_field_id",
						RelatedTableName = "data_source_fields",
						RelatedPrimaryKey = "data_source_field_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "data_view_id",
						RelatedTableName = "data_views",
						RelatedPrimaryKey = "view_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "data_view_filter",
				PrimaryKey = "data_view_filter_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "data_view_id",
						RelatedTableName = "data_views",
						RelatedPrimaryKey = "view_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "data_source_id",
						RelatedTableName = "data_sources",
						RelatedPrimaryKey = "data_source_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "data_views",
				PrimaryKey = "view_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "data_source_id",
						RelatedTableName = "data_sources",
						RelatedPrimaryKey = "data_source_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "data_filter_id",
						RelatedTableName = "data_view_filter",
						RelatedPrimaryKey = "data_view_filter_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "destination_types",
				PrimaryKey = "destination_type_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "device_transaction_log",
				PrimaryKey = "log_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "document_id",
						RelatedTableName = "project_documents",
						RelatedPrimaryKey = "doc_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "file_id",
						RelatedTableName = "files",
						RelatedPrimaryKey = "file_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "device_id",
						RelatedTableName = "user_devices",
						RelatedPrimaryKey = "user_device_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "document_files",
				PrimaryKey = "doc_file_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "doc_id",
						RelatedTableName = "project_documents",
						RelatedPrimaryKey = "doc_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "file_id",
						RelatedTableName = "files",
						RelatedPrimaryKey = "file_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "document_links",
				PrimaryKey = "document_link_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "document_markup_annotation_transactions",
				PrimaryKey = "annotation_transaction_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "document_markup_annotations",
				PrimaryKey = "annotation_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "markup_id",
						RelatedTableName = "document_markups",
						RelatedPrimaryKey = "markup_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "document_markups",
				PrimaryKey = "markup_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "doc_id",
						RelatedTableName = "project_documents",
						RelatedPrimaryKey = "doc_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "doc_file_id",
						RelatedTableName = "document_files",
						RelatedPrimaryKey = "doc_file_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "document_vaults",
				PrimaryKey = "vault_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "engagements",
				PrimaryKey = "engagement_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "company_id",
						RelatedTableName = "customer_companies",
						RelatedPrimaryKey = "company_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "eval_condition_action_targets",
				PrimaryKey = "action_target_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_office_id",
						RelatedTableName = "company_offices",
						RelatedPrimaryKey = "company_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "eval_condition_action_values",
				PrimaryKey = "action_value_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "eval_target_id",
						RelatedTableName = "eval_condition_action_targets",
						RelatedPrimaryKey = "action_target_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "eval_condition_sources",
				PrimaryKey = "condition_source_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_office_id",
						RelatedTableName = "company_offices",
						RelatedPrimaryKey = "company_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "event_attendee",
				PrimaryKey = "event_attendee_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "files",
				PrimaryKey = "file_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "folder_transaction_log",
				PrimaryKey = "folder_transaction_sequence_num",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "folder_id",
						RelatedTableName = "project_folders",
						RelatedPrimaryKey = "folder_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "doc_id",
						RelatedTableName = "project_documents",
						RelatedPrimaryKey = "doc_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "file_id",
						RelatedTableName = "files",
						RelatedPrimaryKey = "file_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "office_id",
						RelatedTableName = "company_offices",
						RelatedPrimaryKey = "company_office_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "new_sheetnum_data",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "notes",
				PrimaryKey = "note_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "notification_templates",
				PrimaryKey = "notification_template_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "ocr_character",
				PrimaryKey = "character_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "ocr_project_files",
				PrimaryKey = "project_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "ocr_projects_to_standardize",
				PrimaryKey = "project_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "ocr_sheet_number",
				PrimaryKey = "word_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "ocr_words",
				PrimaryKey = "word_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "OcrJob",
				PrimaryKey = "ocr_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "OcrLine",
				PrimaryKey = "textline_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "OcrPage",
				PrimaryKey = "page_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "OcrParagraph",
				PrimaryKey = "paragraph_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "OcrTextBlock",
				PrimaryKey = "text_block_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "OcrWord",
				PrimaryKey = "word_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "permissions",
				PrimaryKey = "permission_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "project_documents",
				PrimaryKey = "doc_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "project_documents_published",
				PrimaryKey = "doc_publish_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "doc_id",
						RelatedTableName = "project_documents",
						RelatedPrimaryKey = "doc_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "file_id",
						RelatedTableName = "files",
						RelatedPrimaryKey = "file_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "submission_id",
						RelatedTableName = "project_submissions",
						RelatedPrimaryKey = "project_submission_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "project_eval_criteria",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "project_folder_contents",
				PrimaryKey = "folder_content_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "doc_id",
						RelatedTableName = "project_documents",
						RelatedPrimaryKey = "doc_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "file_id",
						RelatedTableName = "files",
						RelatedPrimaryKey = "file_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "folder_id",
						RelatedTableName = "project_folders",
						RelatedPrimaryKey = "folder_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "project_folders",
				PrimaryKey = "folder_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "submission_id",
						RelatedTableName = "project_submissions",
						RelatedPrimaryKey = "project_submission_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "project_permissions",
				PrimaryKey = "permission_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "project_scope_of_work",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "project_settings",
				PrimaryKey = "project_setting_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "project_sources",
				PrimaryKey = "project_source_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "project_submissions",
				PrimaryKey = "project_submission_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "project_transaction_log",
				PrimaryKey = "project_transaction_seq_num",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "projects",
				PrimaryKey = "project_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_admin_user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "source_sys_type_id",
						RelatedTableName = "source_system_types",
						RelatedPrimaryKey = "source_sys_type_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "SD_dashboards",
				PrimaryKey = "dashboard_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "SD_device_dashboards",
				PrimaryKey = "device_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "device_id",
						RelatedTableName = "SD_devices",
						RelatedPrimaryKey = "device_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "SD_devices",
				PrimaryKey = "device_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "shared_domains",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "shared_projects",
				PrimaryKey = "share_user_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "sheet_name_word_library",
				PrimaryKey = "sheet_name_word",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "sheet_number_library",
				PrimaryKey = "sheet_number",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "sheetnum_candidates",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "sheetnum_candidates_1",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "sheetnum_candidates_3",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "source_system_access_log",
				PrimaryKey = "customer_source_system_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "source_system_types",
				PrimaryKey = "source_sys_type_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "spec_sections_scope_of_work",
				PrimaryKey = "section_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "subscription_transactions",
				PrimaryKey = "transaction_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "system_attributes",
				PrimaryKey = "system_attribute_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "system_products",
				PrimaryKey = "product_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "system_settings",
				PrimaryKey = "system_setting_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "tags",
				PrimaryKey = "tag_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "text_queries",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "user_activity_log",
				PrimaryKey = "user_activity_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "document_id",
						RelatedTableName = "project_documents",
						RelatedPrimaryKey = "doc_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "file_id",
						RelatedTableName = "files",
						RelatedPrimaryKey = "file_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "user_companies",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "user_contacts",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "user_device_contents",
				PrimaryKey = "user_published_file_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "user_devices",
				PrimaryKey = "user_device_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "user_favorites",
				PrimaryKey = "",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "user_notifications",
				PrimaryKey = "user_notification_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "project_id",
						RelatedTableName = "projects",
						RelatedPrimaryKey = "project_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "submission_id",
						RelatedTableName = "project_submissions",
						RelatedPrimaryKey = "project_submission_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "user_settings",
				PrimaryKey = "user_setting_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_id",
						RelatedTableName = "users",
						RelatedPrimaryKey = "user_id"
					},
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "user_device_id",
						RelatedTableName = "user_devices",
						RelatedPrimaryKey = "user_device_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "users",
				PrimaryKey = "user_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "valid_disciplines",
				PrimaryKey = "discipline_id",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
					new ApiDatabaseTableRelation()
					{
						ForeignKey = "customer_id",
						RelatedTableName = "customers",
						RelatedPrimaryKey = "customer_id"
					}
				}
			},
			new ApiDatabaseTable()
			{
				TableName = "valid_patterns",
				PrimaryKey = "pattern",
				ForeignKeys = new List<ApiDatabaseTableRelation>()
				{
				}
			}
		};
	}
}
