using UnityEngine;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.InventoryEngine
{	
	[SelectionBase]
	/// <summary>
	/// A component that handles the visual representation of an Inventory, allowing the user to interact with it
	/// </summary>
	public class InventoryDisplay : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
        [Header("������")]
        [MMInformation("InventoryDisplay���������ӻ��ֿ����ݡ�����ָ��Ҫ��ʾ�Ĳֿ����ơ�", MMInformationAttribute.InformationType.Info, false)]
        [MMLabel("Ŀ��ֿ�����")]
        public string TargetInventoryName = "MainInventory";
        [MMLabel("���ID")]
        public string PlayerID = "Player1";

        protected Inventory _targetInventory = null;

        /// <summary>
        /// Grabs the target inventory based on its name
        /// </summary>
        /// <value>The target inventory.</value>
        public Inventory TargetInventory 
		{ 
			get 
			{ 
				if (TargetInventoryName==null)
				{
					return null;
				}
				if (_targetInventory == null)
				{
					foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
					{
						if ((inventory.name == TargetInventoryName) && (inventory.PlayerID == PlayerID))
						{
							_targetInventory = inventory;
						}
					}	
				}
				return _targetInventory;
			}
		}
		
		
		public struct ItemQuantity
		{
			public string ItemID;
			public int Quantity;

			public ItemQuantity(string itemID, int quantity)
			{
				ItemID = itemID;
				Quantity = quantity;
			}
		}

        [Header("�ֿ�ߴ�")]
        [MMInformation("�ֿ���ʾ���������г�����Ʒ���ݡ�������/�����󣬵��'inspector�ײ���'�Զ�����'��ťԤ��Ч����", MMInformationAttribute.InformationType.Info, false)]
        [MMLabel("����")]
        public int NumberOfRows = 3;

        [MMLabel("����")]
        public int NumberOfColumns = 2;

        /// �ֿ�������
        public virtual int InventorySize { get { return NumberOfRows * NumberOfColumns; } set { } }

        [Header("װ������")]
        [MMInformation("����Ⱦװ���ֿ⣬���ѡ��ֿ⣨ͨ��Ϊ���ֿ⣩����ָ������װ������Ʒ���", MMInformationAttribute.InformationType.Info, false)]
        [MMLabel("ȡ��ʱ���زֿ�")]
		[Tooltip("ȡ��װ��ʱ���ص�Ŀ��ֿ�")]        
		public InventoryDisplay TargetChoiceInventory;
        [MMLabel("��Ʒ���")]
        public ItemClasses ItemClass;

        [Header("��Ϊ����")]
        [MMInformation("����ʱ���ƿղ�ۣ��������ء������ƶ�ʱ��ҿ����ƶ���ť����Ʒ���뱾�ֿ⡣", MMInformationAttribute.InformationType.Info, false)]

        [MMLabel("���ƿղ��")]
        public bool DrawEmptySlots = true;

        [MMLabel("����������Ʒ")]
        public bool AllowMovingObjectsToThisInventory = false;

        [Header("�ֿⲼ��")]
        [MMInformation("����ֿ����߿�����֮��ļ�ࡣ", MMInformationAttribute.InformationType.Info, false)]

        [MMLabel("�ϱ߾�")]
        public int PaddingTop = 20;

        [MMLabel("�ұ߾�")]
        public int PaddingRight = 20;

        [MMLabel("�±߾�")]
        public int PaddingBottom = 20;

        [MMLabel("��߾�")]
        public int PaddingLeft = 20;

        [Header("�������")]
        [MMInformation(
         "���'�Զ�����'��ťʱ���ֿ⽫�����׼����ʾ���ݡ��ɶ����۳ߴ硢�߾༰����״̬ͼ��",
         MMInformationAttribute.InformationType.Info, false)]

        [MMLabel("���Ԥ����")]
        public InventorySlot SlotPrefab;

        [MMLabel("��۳ߴ�")]
        public Vector2 SlotSize = new Vector2(50, 50);

        [MMLabel("ͼ��ߴ�")]
        public Vector2 IconSize = new Vector2(30, 30);

        [MMLabel("��ۼ��")]
        public Vector2 SlotMargin = new Vector2(5, 5);

        [MMLabel("�ղ��ͼ��")]
        public Sprite EmptySlotImage;

        [MMLabel("���ò��ͼ��")]
        public Sprite FilledSlotImage;

        [MMLabel("�������ͼ��")]
        public Sprite HighlightedSlotImage;

        [MMLabel("���²��ͼ��")]
        public Sprite PressedSlotImage;

        [MMLabel("���ò��ͼ��")]
        public Sprite DisabledSlotImage;

        [MMLabel("�ƶ���ͼ��")]
        public Sprite MovedSlotImage;

        [MMLabel("ͼ������")]
        public Image.Type SlotImageType;

        [Header("��������")]
        [MMInformation("�������õ���ϵͳ�������ü���/�ֱ��ڲ�ۼ��ƶ�������������ʱ�Ƿ��Զ���ý��㣨���ֿ�ͨ�����ã���", MMInformationAttribute.InformationType.Info, false)]

        [MMLabel("���õ���")]
        public bool EnableNavigation = true;

        [MMLabel("����ʱ�۽�")]
        public bool GetFocusOnStart = false;


        [Header("�����ı�")]
        [MMInformation("���òֿ����������ʾ���ݡ����塢�ֺź���ɫ����ʽ��", MMInformationAttribute.InformationType.Info, false)]

        [MMLabel("��ʾ����")]
        public bool DisplayTitle = true;

        [MMLabel("��������")]
        public string Title;
        /// the font used to display the quantity
        [MMLabel("��������")]
        public TMPro.TMP_FontAsset TitleFont;

        [MMLabel("�����ֺ�")]
        public int TitleFontSize = 20;

        [MMLabel("������ɫ")]
        public Color TitleColor = Color.black;

        [MMLabel("����ƫ��")]
        public Vector3 TitleOffset = Vector3.zero;
        /// where the quantity should be displayed
		[MMLabel("�������")]
        public TMPro.TextAlignmentOptions TitleAlignment = TMPro.TextAlignmentOptions.BottomRight;

        [Header("�����ı�")]
        [MMInformation("�ѵ���Ʒ������/ҩˮ������ʾ���������������ı������塢��ɫ��λ�á�", MMInformationAttribute.InformationType.Info, false)]

        /// the font used to display the quantity
        [MMLabel("��������")]
        public TMPro.TMP_FontAsset QtyFont;
        [MMLabel("�����ֺ�")]
        public int QtyFontSize = 12;

        [MMLabel("������ɫ")]
        public Color QtyColor = Color.black;

        [MMLabel("�����߾�")]
        public float QtyPadding = 10f;

        /// where the quantity should be displayed
		[MMLabel("��������")]
        public TMPro.TextAlignmentOptions QtyAlignment = TMPro.TextAlignmentOptions.BottomRight;

        [Header("�ֿ⵼��")]
        [MMInformation("InventoryInputManager�ṩ�ֿ�䵼�����ܡ����尴����/�²ֿⰴťʱ����תĿ�ꡣ", MMInformationAttribute.InformationType.Info, false)]
        [MMLabel("��һ���ֿ�")]
        public InventoryDisplay PreviousInventory;
        [MMLabel("��һ���ֿ�")]
        public InventoryDisplay NextInventory;

        /// the grid layout used to display the inventory in rows and columns
        public virtual GridLayoutGroup InventoryGrid { get; protected set; }
		/// the gameobject used to display the inventory's name
		public virtual InventoryDisplayTitle InventoryTitle { get; protected set; }
		/// the main panel
		public virtual RectTransform InventoryRectTransform { get { return GetComponent<RectTransform>(); }}
		/// an internal list of slots
		public virtual List<InventorySlot> SlotContainer { get; protected set; }	
		/// the inventory the focus should return to after an action
		public virtual InventoryDisplay ReturnInventory { get; protected set; }	
		/// whether this inventory display is open or not
		public virtual bool IsOpen { get; protected set; }
		
		public virtual bool InEquipSelection { get; set; }

		/// the item currently being moved

		public static InventoryDisplay CurrentlyBeingMovedFromInventoryDisplay;
		public static int CurrentlyBeingMovedItemIndex = -1;
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			CurrentlyBeingMovedFromInventoryDisplay = null;
			CurrentlyBeingMovedItemIndex = -1;
		}

		protected List<ItemQuantity> _contentLastUpdate;	
		protected List<int> _comparison;	
		protected SpriteState _spriteState = new SpriteState();
		protected InventorySlot _currentlySelectedSlot;
		protected InventorySlot _slotPrefab = null;

        /// <summary>
        /// Creates and sets up the inventory display (usually called via the inspector's dedicated button)
        /// ���������ÿ����ʾ��ͨ��ͨ���������ר�ð�ť���ã�
        /// </summary>
        public virtual void SetupInventoryDisplay()
		{
			if (TargetInventoryName == "")
			{
				Debug.LogError("The " + this.name + " Inventory Display doesn't have a TargetInventoryName set. You need to set one from its inspector, matching an Inventory's name.");
				return;
			}

			if (TargetInventory == null)
			{
				Debug.LogError("The " + this.name + " Inventory Display couldn't find a TargetInventory. You either need to create an inventory with a matching inventory name (" + TargetInventoryName + "), or set that TargetInventoryName to one that exists.");
				return;
			}

			// if we also have a sound player component, we set it up too
			if (this.gameObject.MMGetComponentNoAlloc<InventorySoundPlayer>() != null)
			{
				this.gameObject.MMGetComponentNoAlloc<InventorySoundPlayer> ().SetupInventorySoundPlayer ();
			}

			InitializeSprites();
			AddGridLayoutGroup();
			DrawInventoryTitle();
			ResizeInventoryDisplay ();
			DrawInventoryContent();
		}

		/// <summary>
		/// On Awake, initializes the various lists used to keep track of the content of the inventory
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

        /// <summary>
        /// Initializes lists and redraws the inventory display
        /// ��ʼ���б����»��ƿ����ʾ����
        /// </summary>
        public virtual void Initialization(bool forceRedraw = false)
		{
			_contentLastUpdate = new List<ItemQuantity>();		
			SlotContainer = new List<InventorySlot>() ;		
			_comparison = new List<int>();
			if (!TargetInventory.Persistent || forceRedraw)
			{
				RedrawInventoryDisplay();
			}
		}

		/// <summary>
		/// Redraws the inventory display's contents when needed (usually after a change in the target inventory)
		/// </summary>
		protected virtual void RedrawInventoryDisplay()
		{
			InitializeSprites();
			AddGridLayoutGroup();
			DrawInventoryContent();		
			FillLastUpdateContent();	
		}

		/// <summary>
		/// Initializes the sprites.
		/// ��ʼ��
		/// </summary>
		protected virtual void InitializeSprites()
		{
			// we create a spriteState to specify our various button states
			_spriteState.disabledSprite = DisabledSlotImage;
			_spriteState.selectedSprite = HighlightedSlotImage;
			_spriteState.highlightedSprite = HighlightedSlotImage;
			_spriteState.pressedSprite = PressedSlotImage;
		}

        /// <summary>
        /// Adds and sets up the inventory title child object
        /// ��Ӳ����ÿ������Ӷ���
        /// </summary>
        protected virtual void DrawInventoryTitle()
		{
			if (!DisplayTitle)
			{
				return;
			}
			if (GetComponentInChildren<InventoryDisplayTitle>() != null)
			{
				if (!Application.isPlaying)
				{
					foreach (InventoryDisplayTitle title in GetComponentsInChildren<InventoryDisplayTitle>())
					{
						DestroyImmediate(title.gameObject);
					}
				}
				else
				{
					foreach (InventoryDisplayTitle title in GetComponentsInChildren<InventoryDisplayTitle>())
					{
						Destroy(title.gameObject);
					}
				}
			}
			GameObject inventoryTitle = new GameObject();
			InventoryTitle = inventoryTitle.AddComponent<InventoryDisplayTitle>();
			inventoryTitle.name="InventoryTitle";
			inventoryTitle.GetComponent<RectTransform>().SetParent(this.transform);
			inventoryTitle.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
			inventoryTitle.GetComponent<RectTransform>().localPosition = TitleOffset;
			inventoryTitle.GetComponent<RectTransform>().localScale = Vector3.one;
			InventoryTitle.text = Title;
			InventoryTitle.color = TitleColor;
			InventoryTitle.font = TitleFont;
			InventoryTitle.fontSize = TitleFontSize;
			InventoryTitle.SetTMPAlignment( TitleAlignment );
			InventoryTitle.raycastTarget = false;
		}

		/// <summary>
		/// Adds a grid layout group if there ain't one already
		/// </summary>
		protected virtual void AddGridLayoutGroup()
		{
			if (GetComponentInChildren<InventoryDisplayGrid>() == null)
			{
				GameObject inventoryGrid=new GameObject("InventoryDisplayGrid");
				inventoryGrid.transform.parent=this.transform;
				inventoryGrid.transform.position=transform.position;
				inventoryGrid.transform.localScale=Vector3.one;
				inventoryGrid.AddComponent<InventoryDisplayGrid>();
				InventoryGrid = inventoryGrid.AddComponent<GridLayoutGroup>();
			}
			if (InventoryGrid == null)
			{
				InventoryGrid = GetComponentInChildren<GridLayoutGroup>();
			}
			InventoryGrid.padding.top = PaddingTop;
			InventoryGrid.padding.right = PaddingRight;
			InventoryGrid.padding.bottom = PaddingBottom;
			InventoryGrid.padding.left = PaddingLeft;
			InventoryGrid.cellSize = SlotSize;
			InventoryGrid.spacing = SlotMargin;
		}

		/// <summary>
		/// Resizes the inventory panel, taking into account the number of rows/columns, the padding and margin
		/// </summary>
		protected virtual void ResizeInventoryDisplay()
		{

			float newWidth = PaddingLeft + SlotSize.x * NumberOfColumns + SlotMargin.x * (NumberOfColumns-1) + PaddingRight;
			float newHeight = PaddingTop + SlotSize.y * NumberOfRows + SlotMargin.y * (NumberOfRows-1) + PaddingBottom;

			TargetInventory.ResizeArray(NumberOfRows * NumberOfColumns);	

			Vector2 newSize= new Vector2(newWidth,newHeight);
			InventoryRectTransform.sizeDelta = newSize;
			InventoryGrid.GetComponent<RectTransform>().sizeDelta = newSize;
		}

		/// <summary>
		/// Draws the content of the inventory (slots and icons)
		/// </summary>
		protected virtual void DrawInventoryContent ()             
		{            
			if (SlotContainer != null)
			{
				SlotContainer.Clear();
			}
			else
			{
				SlotContainer = new List<InventorySlot>();
			}
			// we initialize our sprites 
			if (EmptySlotImage==null)
			{
				InitializeSprites();
			}
			// we remove all existing slots
			foreach (InventorySlot slot in transform.GetComponentsInChildren<InventorySlot>())
			{	 			
				if (!Application.isPlaying)
				{
					DestroyImmediate (slot.gameObject);
				}
				else
				{
					Destroy(slot.gameObject);
				}				
			}
			// for each slot we create the slot and its content
			for (int i = 0; i < TargetInventory.Content.Length; i ++) 
			{    
				DrawSlot(i);
			}

			if (_slotPrefab != null)
			{
				if (Application.isPlaying)
				{
					Destroy(_slotPrefab.gameObject);
					_slotPrefab = null;
				}
				else
				{
					DestroyImmediate(_slotPrefab.gameObject);
					_slotPrefab = null;
				}	
			}

			if (EnableNavigation)
			{
				SetupSlotNavigation();
			}
		}

		/// <summary>
		/// If the content has changed, we draw our inventory panel again
		/// </summary>
		protected virtual void ContentHasChanged()
		{
			if (!(Application.isPlaying))
			{
				AddGridLayoutGroup();
				DrawInventoryContent();
				#if UNITY_EDITOR
				EditorUtility.SetDirty(gameObject);
				#endif
			}
			else
			{
				if (!DrawEmptySlots)
				{
					DrawInventoryContent();
				}
				else
				{
					UpdateInventoryContent();	
				}
			}
		}

		/// <summary>
		/// Fills the last content of the update.
		/// </summary>
		protected virtual void FillLastUpdateContent()		
		{		
			_contentLastUpdate.Clear();		
			_comparison.Clear();
			for (int i = 0; i < TargetInventory.Content.Length; i ++) 		
			{  		
				if (!InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					_contentLastUpdate.Add(new ItemQuantity(TargetInventory.Content[i].ItemID, TargetInventory.Content[i].Quantity));
				}
				else
				{
					_contentLastUpdate.Add(new ItemQuantity(null,0));	
				}	
			}	
		}

		/// <summary>
		/// Draws the content of the inventory (slots and icons)
		/// </summary>
		protected virtual void UpdateInventoryContent ()             
		{      
			if (_contentLastUpdate == null || _contentLastUpdate.Count == 0)
			{
				FillLastUpdateContent();
			}

			// we compare our current content with the one in storage to look for changes
			for (int i = 0; i < TargetInventory.Content.Length; i ++) 
			{
				if ((TargetInventory.Content[i] == null) && (_contentLastUpdate[i].ItemID != null))
				{
					_comparison.Add(i);
				}
				if ((TargetInventory.Content[i] != null) && (_contentLastUpdate[i].ItemID == null))
				{
					_comparison.Add(i);
				}
				if ((TargetInventory.Content[i] != null) && (_contentLastUpdate[i].ItemID != null))
				{
					if ((TargetInventory.Content[i].ItemID != _contentLastUpdate[i].ItemID) || (TargetInventory.Content[i].Quantity != _contentLastUpdate[i].Quantity))
					{
						_comparison.Add(i);
					}
				}
			}
			if (_comparison.Count>0)
			{
				foreach (int comparison in _comparison)
				{
					UpdateSlot(comparison);
				}
			} 	    
			FillLastUpdateContent();
		}

		/// <summary>
		/// Updates the slot's content and appearance
		/// </summary>
		/// <param name="i">The index.</param>
		protected virtual void UpdateSlot(int i)
		{
			
			if (SlotContainer.Count < i)
			{
				Debug.LogWarning ("It looks like your inventory display wasn't properly initialized. If you're not triggering any Load events, you may want to mark your inventory as non persistent in its inspector. Otherwise, you may want to reset and empty saved inventories and try again.");
			}

			if (SlotContainer.Count <= i)
			{
				return;
			}
			
			if (SlotContainer[i] == null)
			{
				return;
			}
			// we update the slot's bg image
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				SlotContainer[i].TargetImage.sprite = FilledSlotImage;   
			}
			else
			{
				SlotContainer[i].TargetImage.sprite = EmptySlotImage; 
			}
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				// we redraw the icon
				SlotContainer[i].DrawIcon(TargetInventory.Content[i],i);
			}
			else
			{
				SlotContainer[i].DrawIcon(null,i);
			}
		}

		/// <summary>
		/// Creates the slot prefab to use in all slot creations
		/// </summary>
		protected virtual void InitializeSlotPrefab()
		{
			if (SlotPrefab != null)
			{
				_slotPrefab = Instantiate(SlotPrefab);
			}
			else
			{
				GameObject newSlot = new GameObject();
				newSlot.AddComponent<RectTransform>();

				newSlot.AddComponent<Image> ();
				newSlot.MMGetComponentNoAlloc<Image> ().raycastTarget = true;

				_slotPrefab = newSlot.AddComponent<InventorySlot> ();
				_slotPrefab.transition = Selectable.Transition.SpriteSwap;

				Navigation explicitNavigation = new Navigation ();
				explicitNavigation.mode = Navigation.Mode.Explicit;
				_slotPrefab.GetComponent<InventorySlot> ().navigation = explicitNavigation;

				_slotPrefab.interactable = true;

				newSlot.AddComponent<CanvasGroup> ();
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().alpha = 1;
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().interactable = true;
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().blocksRaycasts = true;
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().ignoreParentGroups = false;
				
				// we add the icon
				GameObject itemIcon = new GameObject("Slot Icon", typeof(RectTransform));
				itemIcon.transform.SetParent(newSlot.transform);
				UnityEngine.UI.Image itemIconImage = itemIcon.AddComponent<Image>();
				_slotPrefab.IconImage = itemIconImage;
				RectTransform itemRectTransform = itemIcon.GetComponent<RectTransform>();
				itemRectTransform.localPosition = Vector3.zero;
				itemRectTransform.localScale = Vector3.one;
				MMGUI.SetSize(itemRectTransform, IconSize);

				// we add the quantity placeholder
				GameObject textObject = new GameObject("Slot Quantity", typeof(RectTransform));
				textObject.transform.SetParent(itemIcon.transform);
				TMPro.TMP_Text textComponent = textObject.AddComponent<TMPro.TextMeshProUGUI>();
				_slotPrefab.QuantityText = textComponent;
				textComponent.font = QtyFont;
				textComponent.fontSize = QtyFontSize;
				textComponent.color = QtyColor;
				textComponent.SetTMPAlignment( QtyAlignment );
				RectTransform textObjectRectTransform = textObject.GetComponent<RectTransform>();
				textObjectRectTransform.localPosition = Vector3.zero;
				textObjectRectTransform.localScale = Vector3.one;
				MMGUI.SetSize(textObjectRectTransform, (SlotSize - Vector2.one * QtyPadding)); 

				_slotPrefab.name = "SlotPrefab";
			}
		}

		/// <summary>
		/// Draws the slot and its content (icon, quantity...).
		/// </summary>
		/// <param name="i">The index.</param>
		protected virtual void DrawSlot(int i)
		{
			if (!DrawEmptySlots)
			{
				if (InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					return;
				}
			}
			
			if ((_slotPrefab == null) || (!_slotPrefab.isActiveAndEnabled))
			{
                //��_slotPrefab��ֵ
                InitializeSlotPrefab();
			}

			InventorySlot theSlot = Instantiate(_slotPrefab);

			theSlot.transform.SetParent(InventoryGrid.transform);
			theSlot.TargetRectTransform.localScale = Vector3.one;
			theSlot.transform.position = transform.position;
			theSlot.name = "Slot "+i;

			// we add the background image
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				theSlot.TargetImage.sprite = FilledSlotImage;   
			}
			else
			{
				theSlot.TargetImage.sprite = EmptySlotImage;      	
			}
			theSlot.TargetImage.type = SlotImageType; 
			theSlot.spriteState=_spriteState;
			theSlot.MovedSprite=MovedSlotImage;
			theSlot.ParentInventoryDisplay = this;
			theSlot.Index=i;

			SlotContainer.Add(theSlot);	

			theSlot.gameObject.SetActive(true)	;

			theSlot.DrawIcon(TargetInventory.Content[i],i);
		}

		/// <summary>
		/// Setups the slot navigation using Unity's GUI built-in system, so that the user can move using the left/right/up/down arrows
		/// </summary>
		protected virtual void SetupSlotNavigation()
		{
			if (!EnableNavigation)
			{
				return;
			}

			for (int i=0; i<SlotContainer.Count;i++)
			{
				if (SlotContainer[i]==null)
				{
					return;
				}
				Navigation navigation = SlotContainer[i].navigation;
				// we determine where to go when going up
				if (i - NumberOfColumns >= 0) 
				{
					navigation.selectOnUp = SlotContainer[i-NumberOfColumns];
				}
				else
				{
					navigation.selectOnUp=null;
				}
				// we determine where to go when going down
				if (i+NumberOfColumns < SlotContainer.Count) 
				{
					navigation.selectOnDown = SlotContainer[i+NumberOfColumns];
				}
				else
				{
					navigation.selectOnDown=null;
				}
				// we determine where to go when going left
				if ((i%NumberOfColumns != 0) && (i>0))
				{
					navigation.selectOnLeft = SlotContainer[i-1];
				}
				else
				{
					navigation.selectOnLeft=null;
				}
				// we determine where to go when going right
				if (((i+1)%NumberOfColumns != 0)  && (i<SlotContainer.Count - 1))
				{
					navigation.selectOnRight = SlotContainer[i+1];
				}
				else
				{
					navigation.selectOnRight=null;
				}
				SlotContainer[i].navigation = navigation;
			}
		}

		/// <summary>		
		/// Sets the focus on the first item of the inventory		
		/// </summary>		
		public virtual void Focus()		
		{
			if (!EnableNavigation)
			{
				return;
			}
			
			if (SlotContainer.Count > 0)
			{
				SlotContainer[0].Select();
			}		

			if (EventSystem.current.currentSelectedGameObject == null)
			{
				InventorySlot newSlot = transform.GetComponentInChildren<InventorySlot>();
				if (newSlot != null)
				{
					EventSystem.current.SetSelectedGameObject (newSlot.gameObject);	
				}
			}			
		}

		/// <summary>
		/// Returns the currently selected inventory slot
		/// </summary>
		/// <returns>The selected inventory slot.</returns>
		public virtual InventorySlot CurrentlySelectedInventorySlot()
		{
			return _currentlySelectedSlot;
		}

		/// <summary>
		/// Sets the currently selected slot
		/// </summary>
		/// <param name="slot">Slot.</param>
		public virtual void SetCurrentlySelectedSlot(InventorySlot slot)
		{
			_currentlySelectedSlot = slot;
		}

		/// <summary>
		/// Goes to the previous (-1) or next (1) inventory, based on the int direction passed in parameter.
		/// </summary>
		/// <param name="direction">Direction.</param>
		public virtual InventoryDisplay GoToInventory(int direction)
		{
			if (direction==-1)
			{
				if (PreviousInventory==null)
				{
					return null;
				}
				PreviousInventory.Focus();
				return PreviousInventory;
			}
			else
			{
				if (NextInventory==null)
				{
					return null;
				}
				NextInventory.Focus();	
				return NextInventory;			
			}
		}

		/// <summary>
		/// Sets the return inventory display
		/// </summary>
		/// <param name="inventoryDisplay">Inventory display.</param>
		public virtual void SetReturnInventory(InventoryDisplay inventoryDisplay)
		{
			ReturnInventory = inventoryDisplay;
		}

		/// <summary>
		/// If possible, returns the focus to the current return inventory focus (after equipping an item, usually)
		/// </summary>
		public virtual void ReturnInventoryFocus()
		{
			if (ReturnInventory == null)
			{
				return;
			}
			else
			{
				InEquipSelection = false;
				ResetDisabledStates();
				ReturnInventory.Focus();
				ReturnInventory = null;
			}
		}

		/// <summary>
		/// Disables all the slots in the inventory display, except those from a certain class
		/// </summary>
		/// <param name="itemClass">Item class.</param>
		public virtual void DisableAllBut(ItemClasses itemClass)
		{
			for (int i=0; i < SlotContainer.Count;i++)
			{
				if (InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					continue;
				}
				if (TargetInventory.Content[i].ItemClass!=itemClass)
				{
					SlotContainer[i].DisableSlot();
				}
			}
		}

		/// <summary>
		/// Enables back all slots (usually after having disabled some of them)
		/// </summary>
		public virtual void ResetDisabledStates()
		{
			for (int i=0; i<SlotContainer.Count;i++)
			{
				SlotContainer[i].EnableSlot();
			}
		}

		/// <summary>
		/// A public method you can use to change the target inventory of this display to a new inventory
		/// </summary>
		/// <param name="newInventoryName"></param>
		public virtual void ChangeTargetInventory(string newInventoryName)
		{
			_targetInventory = null;
			TargetInventoryName = newInventoryName;
			Initialization(true);
		}

		/// <summary>
		/// Catches MMInventoryEvents and acts on them
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			// if this event doesn't concern our inventory display, we do nothing and exit
			if (inventoryEvent.TargetInventoryName != this.TargetInventoryName)
			{
				return;
			}

			if (inventoryEvent.PlayerID != this.PlayerID)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Click:
					ReturnInventoryFocus ();
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Move:
					this.ReturnInventoryFocus();
					UpdateSlot(inventoryEvent.Index);

					break;

				case MMInventoryEventType.ItemUsed:
					this.ReturnInventoryFocus();
					break;
				
				case MMInventoryEventType.EquipRequest:
					if (this.TargetInventory.InventoryType == Inventory.InventoryTypes.Equipment)
					{
						// if there's no target inventory set we do nothing and exit
						if (TargetChoiceInventory == null)
						{
							Debug.LogWarning ("InventoryEngine Warning : " + this + " has no choice inventory associated to it.");
							return;
						}
                        // we disable all the slots that don't match the right type
                        //���ǽ�����������ȷ���Ͳ�ƥ��Ĳ�ۡ�
						TargetChoiceInventory.DisableAllBut (this.ItemClass);
						// we set the focus on the target inventory
						TargetChoiceInventory.Focus ();
						TargetChoiceInventory.InEquipSelection = true;
						// we set the return focus inventory
						TargetChoiceInventory.SetReturnInventory (this);
					}
					break;
				
				case MMInventoryEventType.ItemEquipped:
					ReturnInventoryFocus();
					break;

				case MMInventoryEventType.Drop:
					this.ReturnInventoryFocus ();
					break;

				case MMInventoryEventType.ItemUnEquipped:
					this.ReturnInventoryFocus ();
					break;

				case MMInventoryEventType.InventoryOpens:
					Focus();
					InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
					IsOpen = true;
					EventSystem.current.sendNavigationEvents = true;
					break;

				case MMInventoryEventType.InventoryCloses:
					InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
					EventSystem.current.sendNavigationEvents = false;
					IsOpen = false;
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.ContentChanged:
					ContentHasChanged ();
					break;

				case MMInventoryEventType.Redraw:
					RedrawInventoryDisplay ();
					break;

				case MMInventoryEventType.InventoryLoaded:
					RedrawInventoryDisplay ();
					if (GetFocusOnStart)
					{
						Focus();
					}
					break;
			}
		}

		/// <summary>
		/// On Enable, we start listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

		/// <summary>
		/// On Disable, we stop listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}




  
